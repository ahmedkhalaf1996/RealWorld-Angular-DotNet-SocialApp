using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using backend.Services; 
using backend.interfaces;
using backend.Models;
using backend.Protos;
using Google.Protobuf.WellKnownTypes;

namespace backend.Conrollers;

[Controller]
[Route("/posts")]

public class PostController: Controller {
    private readonly IConfiguration _configuration;
    private readonly IPostService _postService;
        private readonly INotificationService _notificationService;
        private readonly IRealtimeNotificationClient _realtimeNotificationClient;

    public PostController(IPostService postService,   
     INotificationService notificationService,
     IRealtimeNotificationClient realtimeNotificationClient, 
     IConfiguration configuration){
        _postService = postService;
        _notificationService = notificationService;
        _realtimeNotificationClient = realtimeNotificationClient;
        _configuration = configuration;

    }


    [HttpPost]
    [Route(""), Authorize]
    public async Task<IActionResult> CreatePost([FromBody] CraeteOrUpdatePostInterface body){
        var post = new Post{};
        if(body.title == null || body.message == null || body.selectedFile == null){
            return BadRequest(new {message = "proplem with provided body data."});
        }

        post.title = body.title;
        var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
        post.creator = userIDToken;
        post.message = body.message;
        post.selectedFile = body.selectedFile;

        await _postService.CreateOnePostAsync(post);

        if(post == null){
            return BadRequest(new {message = "some thing went worng!."});
        }

        return Ok(new {post= post});
    }


    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetPost([FromRoute] string id){
        if(id is null){
            return BadRequest(new {message = "proplem with provided id"});
        }
        // var post = new PostResponse{};
        var post = await _postService.GetPostByID(id);

        if(post is null) return NotFound(new {message = "post not found", Success = false});

        return Ok(new { post });
    }

    [HttpPost]
    // [Route("{id}/commentPost"), Authorize]
    [Route("{id}/commentPost"), Authorize]
    public async Task<IActionResult> AddComment([FromRoute] string id, [FromBody] CommentBodyInterface body){
        if(body.value is null || id is null){
            return BadRequest(new {message = "proplem with provided body data id or comment value"});
        }
        var post = await _postService.GetPostByID(id);
        if(post is null) return NotFound(new {message = "post not found", Success = false});

        var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
        if(userIDToken is null) return Unauthorized();

        // craeting a comment
        await _postService.CreateComment(id, userIDToken, body.value);


        if(post.creator != null && userIDToken != null ){
        // Call notification Start 
           var user = await _postService.GetUsByid(userIDToken);
            if (user is not null){
                        
            var deat = user.name + " Comment On Your Post";
            var us = new UserIn{name = user.name, imageUrl = user.imageUrl};
            var nofification = new Notification {
                mainuid = post.creator,
                targetid =id,
                deatils = deat,
                senderid = userIDToken,
                user = us
            };
            
              await _notificationService.CreateNotification(nofification);

              // gRPC Realtime notification
              try
              {
                await _realtimeNotificationClient.SendGrpcNotificationAsync(new NotificationGrpcRequest
                {
                    Mainuid = post.creator ?? "",
                    Targetid = id ?? "",
                    Id = Guid.NewGuid().ToString(),
                    Deatils = deat ?? "",
                    Isreded = false,
                    CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                    User = new Usergrpc
                    {
                        Name = user.name ?? "",
                        ImageUrl = user.imageUrl ?? ""
                    }
                    
                });
              }
              catch (Exception ex)
              {
                
               Console.WriteLine($"Error sending gRPC comment notification: {ex.Message}");
              }

            }

            // call nofification end
        }


        // return Ok(new {data=post});
#pragma warning disable CS8604 // Possible null reference argument.
        var updatedPost = await _postService.GetPostWithComments(id);
#pragma warning restore CS8604 // Possible null reference argument.
        return Ok(new {post = updatedPost});
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> SearchForUsersPost([FromQuery] string searchQuery){
        if(searchQuery is null){
          return BadRequest(new {message = "proplem with provided serchquery"});
        }

        var posts = new List<Post>();
        var users = new List<User>();

        (posts, users) = await _postService.Search(searchQuery);

        return Ok(new {posts= posts, user = users});
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetPostsPagenationAsync([FromQuery] int Page, [FromQuery] string id){
        if(id == "undefind") return BadRequest(new {message = "proplem with provided id"});

        var user = await _postService.GetUsByid(id);

        if(user is null || user._id is null){
            return NotFound(new {message = "user with given id is not found."});
        }

        var ides = user.following;
        ides.Add(user._id.ToString());

        return Ok(await _postService.Query(ides, Page));
    }

    [HttpPatch]
    [Route("{id}"), Authorize]
    public async Task<IActionResult> UpdatePost([FromRoute] string id, [FromBody] CraeteOrUpdatePostInterface body){
         if(body.title == null || body.message == null || body.selectedFile == null){
            return BadRequest(new {message = "proplem with provided body data."});
        }

        var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
        if (userIDToken is null){
             return NotFound(new {message = "Not Authorized."});
        }


        var postResponse = await _postService.GetPostByID(id);

        if (postResponse is null){
            return NotFound(new {message = "post with given id is not found.."});
        }

        if (userIDToken != postResponse.creator){
            return Unauthorized(new {message = "Not Authorized. you are not the creator of post"});

        }

        if (body.title is null && body.message is null && body.selectedFile is null)
        {
            return BadRequest(new {message = "proplem with profived body data."});
        }

        // create new post obj
        var postUpdate = new Post
        {
            _id = id,
            creator = postResponse.creator ,
            title = body.title ?? postResponse.title,
            message = body.message ?? postResponse.message,
            selectedFile = body.selectedFile ?? postResponse.selectedFile,
            likes = postResponse.likes,
            createdAt = postResponse.createdAt ?? DateTime.Now,
        };
   

        // upate post
        var upPost = await _postService.UpdatePost(id, postUpdate);
        if (upPost is null){
            return BadRequest(new {message = "can not update the post."});
        }    

        return Ok(new {post = upPost});    
    }

    [HttpPatch]
    [Route("{id}/likePost"), Authorize]
    public async Task<IActionResult> LikeDisLikePost([FromRoute] string id){
        var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
        if (userIDToken is null){
             return NotFound(new {message = "Not Authorized."});
        }
        
        var postResponse = await _postService.GetPostByID(id);
        
        if (postResponse is null){
            return NotFound(new {message = "post with given id is not found.."});
        }

        var likes = postResponse.likes ?? new List<string>();


        if(likes.Contains(userIDToken)){
            likes.Remove(userIDToken);
        } else {
           likes.Add(userIDToken);
            // TODO Call Notification .. notofy the user about the new user like about the post
            if (postResponse.creator != null){
                    var user = new User{};
                user = await _postService.GetUsByid(userIDToken);
                if (user is not null){
                            
                var deat = user.name + " Like Your Post";
                var us = new UserIn{name = user.name, imageUrl = user.imageUrl};
                var nofification = new Notification {
                    mainuid = postResponse.creator,
                    targetid =id,
                    deatils = deat,
                    senderid = userIDToken,
                    user = us
                };
                
                await _notificationService.CreateNotification(nofification);


             // gRPC Realtime notification
              try
              {
                await _realtimeNotificationClient.SendGrpcNotificationAsync(new NotificationGrpcRequest
                {
                    Mainuid = postResponse.creator ?? "",
                    Targetid = id ?? "",
                    Id = Guid.NewGuid().ToString(),
                    Deatils = deat ?? "",
                    Isreded = false,
                    CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                    User = new Usergrpc
                    {
                        Name = user.name ?? "",
                        ImageUrl = user.imageUrl ?? ""
                    }
                    
                });
              }
              catch (Exception ex)
              {
                
               Console.WriteLine($"Error sending gRPC comment notification: {ex.Message}");
              }




            }
            }
        }


        // create new post obj
        var postUpdate = new Post
        {
            _id = id,
            creator = postResponse.creator ,
            title =  postResponse.title,
            message =  postResponse.message,
            selectedFile =  postResponse.selectedFile,
            likes = likes,
            createdAt = postResponse.createdAt ?? DateTime.Now,
        };


        // upate post
#pragma warning disable CS8604 // Possible null reference argument.
        var upPost = await _postService.UpdatePost(id, postUpdate);
#pragma warning restore CS8604 // Possible null reference argument.
        if (upPost is null){
            return BadRequest(new {message = "can not update the post."});
        }    

        var enrichedPost = await _postService.GetPostByID(id);
        return Ok(new {post = enrichedPost});  


    }

    [HttpDelete]
    [Route("{id}"), Authorize]
    public async Task<IActionResult>  DeletePost([FromRoute] string id){
        var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
        if (userIDToken is null){
             return NotFound(new {message = "Not Authorized."});
        }
        
        var post = await _postService.GetPostByID(id);
        
        if (post is null){
            return NotFound(new {message = "post with given id is not found.."});
        }

        if (userIDToken != post.creator){
            return Unauthorized(new {message = "Not Authorized. you are not the creator of post"});
        }

        await _postService.DeletePostAsync(id);
        return Ok(new {message = "post Deleted Successfully."});

    }

[HttpDelete]
[Route("/comments/{postId}/comments/{commentId}"), Authorize] 
public async Task<IActionResult> DeleteComment([FromRoute] string postId, [FromRoute] string commentId)
 {
    var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
    if (userIDToken is null){
             return NotFound(new {message = "Not Authorized."});
    }
 
    var post = await _postService.GetPostByID(postId);
    if (post is null) return NotFound(new {message = "post not found"});

    var postCreatorId = post.creator ?? "unnown";

    try
    {
      var deleted = await _postService.DeleteComment(commentId, userIDToken, postCreatorId);
      if (!deleted) return NotFound(new {message = "comment not found"});

      return Ok(
        new
        {
            message = "Comment Deleted Successfully",
            deletedCommentId = commentId
        });

    } catch (UnauthorizedAccessException)
        {
            return Unauthorized(new {message = "You are not authoreized to delete this comment"});
        }
 }
 



}




