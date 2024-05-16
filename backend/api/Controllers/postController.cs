using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using backend.Services; 
using backend.interfaces;
using backend.Models;

namespace backend.Conrollers;

[Controller]
[Route("/posts")]

public class PostController: Controller {
    private readonly IConfiguration _configuration;
    private readonly PostService _postService;
        private readonly NotificationService _notificationService;

    public PostController(PostService postService,    NotificationService notificationService,IConfiguration configuration){
        _postService = postService;
        _notificationService = notificationService;

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
        var post = new Post{};
        post = await _postService.GetPostByID(id);

        if(post is null) return NotFound(new {message = "post not found", Success = false});

        return Ok(new {post = post});
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

        post.comments.Add(body.value);

        var npost = await _postService.UpdatePost(id, post);

        if(npost is null) return NotFound(new {message = "proplem with prodived value", Success = false});

        var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
        if(post.creator != null && userIDToken != null ){
        // Call notification Start 
            var user = new User{};
            user = await _postService.GetUsByid(userIDToken);
            if (user is not null){
                        
            var deat = user.name + " Comment On Your Post";
            var us = new UserIn{name = user.name, avatar = user.imageUrl};
            var nofification = new Notification {
                mainuid = post.creator,
                targetid =id,
                deatils = deat,
                user = us
            };
            
              await _notificationService.CreateNotification(nofification);

            }

            // call nofification end
        }


        return Ok(new {data=post});

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

        var user = new User{};
        user = await _postService.GetUsByid(id);

        if(user is null || user._id is null){
            return NotFound(new {message = "user with given id is not found."});
        }

        var ides = user.following;
        ides.Add(user._id.ToString());

        return Ok(_postService.Query(ides, Page));
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


        var post = new Post{};
        post = await _postService.GetPostByID(id);

        if (post is null){
            return NotFound(new {message = "post with given id is not found.."});
        }

        if (userIDToken != post.creator){
            return Unauthorized(new {message = "Not Authorized. you are not the creator of post"});

        }

        // add the new data
        post.title = body.title;
        post.message = body.message;
        post.selectedFile = body.selectedFile;

        // upate post
        var upPost = await _postService.UpdatePost(id, post);
        if (upPost is null){
            return BadRequest(new {message = "can not update the post."});
        }    

        return Ok(new {post = post});    
    }

    [HttpPatch]
    [Route("{id}/likePost"), Authorize]
    public async Task<IActionResult> LikeDisLikePost([FromRoute] string id){
        var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
        if (userIDToken is null){
             return NotFound(new {message = "Not Authorized."});
        }
        
        var post = new Post{};
        post = await _postService.GetPostByID(id);
        
        if (post is null){
            return NotFound(new {message = "post with given id is not found.."});
        }

        if(post.likes.Contains(userIDToken)){
            post.likes.Remove(userIDToken);
        } else {
            post.likes.Add(userIDToken);
            // TODO Call Notification .. notofy the user about the new user like about the post
            if (post.creator != null){
                    var user = new User{};
                user = await _postService.GetUsByid(userIDToken);
                if (user is not null){
                            
                var deat = user.name + " Like Your Post";
                var us = new UserIn{name = user.name, avatar = user.imageUrl};
                var nofification = new Notification {
                    mainuid = post.creator,
                    targetid =id,
                    deatils = deat,
                    user = us
                };
                
                await _notificationService.CreateNotification(nofification);

            }
            }
        }

        // upate post
        var upPost = await _postService.UpdatePost(id, post);
        if (upPost is null){
            return BadRequest(new {message = "can not update the post."});
        }    

        return Ok(new {post = post});  


    }

    [HttpDelete]
    [Route("{id}"), Authorize]
    public async Task<IActionResult>  DeletePost([FromRoute] string id){
        var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
        if (userIDToken is null){
             return NotFound(new {message = "Not Authorized."});
        }
        
        var post = new Post{};
        post = await _postService.GetPostByID(id);
        
        if (post is null){
            return NotFound(new {message = "post with given id is not found.."});
        }

        if (userIDToken != post.creator){
            return Unauthorized(new {message = "Not Authorized. you are not the creator of post"});
        }

        await _postService.DeletePostAsync(id);
        return Ok(new {message = "post Deleted Successfully."});

    }

}


 
