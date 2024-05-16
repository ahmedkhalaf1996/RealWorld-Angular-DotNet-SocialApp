using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using backend.Services;
using backend.Models;
using backend.interfaces;
using Microsoft.AspNetCore.Authorization;


namespace backend.Conrollers;


[Controller]
[Route("/user")]

public class UserController: Controller {
    private readonly IConfiguration _configuration;
    private readonly UserService _UserService;
    private readonly NotificationService _notificationService;
    
    public UserController(UserService userService, 
                        NotificationService notificationService,
                        IConfiguration configuration){
        _UserService = userService;
        _notificationService = notificationService;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("signup")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateUserInterface body){
        var user =  new User{};
        if(body.firstName == null || body.lastName == null || body.email == null || body.password == null){
            return BadRequest(new {message = "proplem with provided body data."});
        }

        user.name = body.firstName + body.lastName;
        user.email = body.email;
        user.password = user.EncryptPasswordBase64(body.password);

        var checkuser = await _UserService.GetUserByEmail(body.email);
        
        if(checkuser is not null) {
            return BadRequest(new {message = "User Already Exist."});
        }   

        await _UserService.CreateAsync(user);

        // create token
        var claims = new List<Claim> 
        {
            new Claim(JwtRegisteredClaimNames.Sub, user._id ?? throw new InvalidOperationException()),
            new Claim(ClaimTypes.Name, user.name ?? throw new InvalidOperationException()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user._id.ToString())  
        };


        var tokenSecret = _configuration.GetValue<string>("JwtSecret:Secret");

        var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret ?? throw new IndexOutOfRangeException()));
        var creds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: "https://localhost:5000",
            audience: "https://localhost:5000",
            claims:claims,
            expires: expires,
            signingCredentials: creds
        );

        return Ok(new {result = user, token = new JwtSecurityTokenHandler().WriteToken(token)});
    }

   
   [HttpPost]
   [Route("signin")]
   public async Task<IActionResult> LogInUser([FromBody] LoginInterface body) {
    if (body.email == null || body.password == null){
        return BadRequest(new {message = "proplem with provided body data."});
   }


   var user = await _UserService.GetUserByEmail(body.email);
   var decodedPassword = user?.DecryptPasswordBase64(user.password);
   if(user is null){
    return NotFound();
   } else if (decodedPassword != body.password){
     return BadRequest(new {message = "given email Or Password not correct."});
   } else {
    // sucess
    // create token
    var claims = new List<Claim> 
    {
        new Claim(JwtRegisteredClaimNames.Sub, user._id ?? throw new InvalidOperationException()),
        new Claim(ClaimTypes.Name, user.name ?? throw new InvalidOperationException()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user._id.ToString())  
    };


    var tokenSecret = _configuration.GetValue<string>("JwtSecret:Secret");

    var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret ?? throw new IndexOutOfRangeException()));
    var creds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);
    var expires = DateTime.Now.AddHours(1);

    var token = new JwtSecurityToken(
        issuer: "https://localhost:5000",
        audience: "https://localhost:5000",
        claims:claims,
        expires: expires,
        signingCredentials: creds
    );

     return Ok(new {result = user, token = new JwtSecurityTokenHandler().WriteToken(token)});
   }
   }


    [HttpGet]
    [Route("getUser/{id}")]
    public async Task<IActionResult> GetUserById([FromRoute] string id){
        try {
            var user = new  User{};

            user = await _UserService.GetUserByID(id);

            if (user is null) {
                return NotFound(new {message= "User with gien id is not found!."});
            }

            // TODO return also the user posts
            return Ok( new { user = user, posts = Array.Empty<object>()});
        } 
        catch 
        {
            return BadRequest(new {message = " some thing wet worng!."});
        }
    }



    [HttpPatch]
    [Route("Update/{id}"), Authorize]
    public async Task<IActionResult> UpdateUser([FromRoute] string id , [FromBody] UpdateUserInterface body){
        // check autoriZation
        var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIDToken?.ToString() != id){
            return Unauthorized(new {message = "you are not authorized to update this account."});
        }

        // body validation.
            
        if(body.name == null || body.imageUrl == null || body.bio == null ){
                return BadRequest(new {message = "proplem with provided body data."});
        }

         var user = new User{};
         user = await _UserService.GetUserByID(id);
         if (user is null){
            return NotFound(new {message = "User with given id is not found."});
         }

         // 
         user.name = body.name;
         user.imageUrl = body.imageUrl;
         user.bio = body.bio;

        // update user
        var upUser = await _UserService.UpdateUser(id, user);
        if (upUser is null){
            return NotFound(new {message = "can Not Update the user."});
        }

        return Ok(new {user = user});
    }


    [HttpPatch]
    [Route("{id}/following"), Authorize]
    public async Task<IActionResult>  Following([FromRoute] string id){
        if (id == null){
            return BadRequest(new {message = "proplem with provided id data."});
        }
        try
        {
            var user2 = await _UserService.GetUserByID(id);
            if (user2 is null || user2._id is null ) return NotFound (new {Message = "user Not found", Success = false});

            var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIDToken == null){
                return BadRequest(new {message = "proplem with provided id data of token."});
            }

            var user1 = await _UserService.GetUserByID(userIDToken.ToString());

            if (user1 is null || user1._id is null ) return NotFound (new {Message = "user Not found", Success = false});

            if(user1.following == null){
                user1.following = new List<string>{};
            }

            if(user2.followers == null){
                user2.followers = new List<string>{};
            }

            var fo = user1.following;
            var fo2 = user2.followers;

            if (fo.Contains(id)){
                fo.Remove(id);
                user1.following = fo;
                fo2.Remove(user1._id);
                user2.followers = fo2;
            } else {
                fo.Add(id);
                user1.following = fo;
                fo2.Add(user1._id);
                user2.followers = fo2;
                // Call notification Start 
                var deat = user1.name + " Start Following You";
                var us = new UserIn{name = user1.name, avatar = user1.imageUrl};
                var nofification = new Notification {
                    mainuid = user2._id,
                    targetid = user1._id,
                    deatils = deat,
                    user = us
                };

                await _notificationService.CreateNotification(nofification);
                // call nofification end
            }

            await _UserService.UpdateUser(user1._id.ToString(), user1);
            await _UserService.UpdateUser(user2._id.ToString(), user2);


            return Ok(new {
                user1 = user1,
                user2 = user2, 
                Succes = true,
                Message = "Successfully."
            });
        }
        catch (Exception ex)
        {
            
            return BadRequest(new {Message = ex.Message, Success = false});
        }
    }


    [HttpGet]
    [Route("getSug")]
    public async Task<IActionResult> GetSugUsers([FromQuery] string id)
    {
        try
        {
            if(id == "undefined") return BadRequest(new {Message = "id is undefined ", Success = false});

            var mainUser = await _UserService.GetUserByID(id);
            if (mainUser is null) return  NotFound(new {Message = "user not found! ", Success = false});
        
            var FollowingList = mainUser.following;
            if (FollowingList is null) return  NotFound(new {Message = "null follwing list for  user ", Success = false});

            var FoloUsersList = new List<User>{};
            foreach( var Uid in FollowingList)
            {
                var getuserFollwoing = await _UserService.GetUserByID(Uid);
                if (getuserFollwoing != null){
                     FoloUsersList.Add(getuserFollwoing);
                }
            }
        
        // start use f list
        var usersidesfrosug = new List<string>{};
        var FinalUsers = new List<User>{};
        foreach (var us in FoloUsersList ){
            if (us.followers != null && mainUser._id != null){
                foreach( var ids in us.followers){
                    if (usersidesfrosug.Contains(ids) | ids == mainUser._id.ToString()) continue;
                    var gus = await _UserService.GetUserByID(ids);
                    if (gus != null) FinalUsers.Add(gus);
                    usersidesfrosug.Add(ids);
                }
            }
            // following
            if (us.following != null && mainUser._id != null){
                foreach( var ids in us.following){
                    if (usersidesfrosug.Contains(ids) | ids == mainUser._id.ToString()) continue;
                    var gus = await _UserService.GetUserByID(ids);
                    if (gus != null) FinalUsers.Add(gus);
                    usersidesfrosug.Add(ids);
                }
            }

   
        }

         // return the result 
            return Ok (new {
                Users = FinalUsers,
                Success = true,
                 Message = "Successfully"
            });
        }
        catch ( Exception ex)
        {
            
            return BadRequest(new {Message = ex.Message, Success = false});
        }
    }

    [HttpDelete]
    [Route("delete/{id}"), Authorize]
    public async Task<IActionResult> Delete([FromRoute] string id) {
        var userIDToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userIDToken?.ToString() != id){
            return Unauthorized(new {message = " You are not authorized to detelte this account."});
        }
        await _UserService.DeleteAsync(id);
        return Ok();
    }

}





