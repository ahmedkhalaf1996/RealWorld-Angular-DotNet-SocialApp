

using System.Net;
using System.Net.Http.Json;
using api.Tests.Base;
using backend.interfaces;
using backend.Models;
using MongoDB.Bson;
using Moq;

namespace api.Tests.Controllers
{
    public class UserControllerTests : IntegrationTestBase
    {
        public UserControllerTests(ApiTestFixTure fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Signup_MissingData_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var body = new CreateUserInterface { firstName = "A" };

            var response = await client.PostAsJsonAsync("/user/signup", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task Signup_whenUserExists_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var body = new CreateUserInterface
            {
                firstName = "A",
                lastName = "B",
                email = "a@b.com",
                password = "pass555"
            };

            _mockUserService.Setup(s => s.GetUserByEmail(body.email)).ReturnsAsync(new User {_id = "exists"});

            var response = await client.PostAsJsonAsync("/user/signup", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact] 
        public async Task Signup_valid_ReturnsOK()
        {
            var client = _factory.CreateClient();
            var body = new CreateUserInterface
            {
                firstName = "A",
                lastName = "B",
                email = "a@b.com",
                password = "pass555"
            };
            _mockUserService.Setup(s => s.GetUserByEmail(body.email)).ReturnsAsync((User?)null);
            _mockUserService
                .Setup(s => s.CreateAsync(It.IsAny<User>()))
                .Callback<User>(u => u._id = "new-user-id")
                .Returns(Task.CompletedTask);
            var response = await client.PostAsJsonAsync("/user/signup", body);
            // Console.WriteLine($"Status Code:  {response.StatusCode} ({(int)response.StatusCode})");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockUserService.Verify(s => s.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Signin_MissingData_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var body = new LoginInterface {email = "a@b.com"};

            var response = await client.PostAsJsonAsync("/user/signin", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task Signin_UserNotFound_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            var body = new LoginInterface {email = "a@b.com", password = "pass555"};

            _mockUserService.Setup(s => s.GetUserByEmail(body.email)).ReturnsAsync((User?)null);

            var response = await client.PostAsJsonAsync("/user/signin", body);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Signin_WrongPassword_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var body = new LoginInterface {email = "a@b.com", password = "pass555"};
            var user = new User
            {
                _id = "user1",
                name = "User 1",
                email = "a@b.com",
                password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("otherpass"))
            };

            _mockUserService.Setup(s => s.GetUserByEmail(body.email)).ReturnsAsync(user);

            var response = await client.PostAsJsonAsync("/user/signin", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Signin_Valid_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var body = new LoginInterface {email = "a@b.com", password = "pass555"};
            var user = new User
            {
                _id = "user1",
                name = "User 1",
                email = "a@b.com",
                password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("pass555"))
            };

            _mockUserService.Setup(s => s.GetUserByEmail(body.email)).ReturnsAsync(user);

            var response = await client.PostAsJsonAsync("/user/signin", body);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetUserById_Notfound_RetunNotfound()
        {
            var client = _factory.CreateClient();
            _mockUserService.Setup(s => s.GetUserByID("missing")).ReturnsAsync((User?)null);

            var response = await client.GetAsync("/user/getUser/missing?page=1");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetUserById_RetunsOkWithPosts()
        {
            var client = _factory.CreateClient();
            var userId = "user123";
            _mockUserService.Setup(s => s.GetUserByID(userId)).ReturnsAsync(new User {_id = userId});
            _mockPostService.Setup(s => s.Query(It.IsAny<List<string>>(), It.IsAny<int?>()))
                            .ReturnsAsync(new {data = new List<object>()});
            
            var response = await client.GetAsync($"/user/getUser/{userId}?page=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
            var json = await response.Content.ReadAsStringAsync(); 
            // Console.WriteLine(json);
            Assert.Contains("posts", json);
        }

        [Fact]
        public async Task UpdateUser_Unauthorized_ReturnsUnAuthorized()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("x-test-UserId", "other");

            var body = new UpdateUserInterface { name = "n", imageUrl = "i", bio = "b" };

            var response = await client.PatchAsJsonAsync("/user/Update/user1", body);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_Valid_ReturnsOk()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("x-test-UserId", "user1");

            var body = new UpdateUserInterface { name = "n", imageUrl = "i", bio = "b" };
            _mockUserService.Setup(s => s.GetUserByID("user1")).ReturnsAsync(new User {_id = "user1"});
            _mockUserService.Setup(s => s.UpdateUser("user1", It.IsAny<User>())).ReturnsAsync(new User { _id = "user1"});
            var response = await client.PatchAsJsonAsync("/user/Update/user1", body);
            var json  = await response.Content.ReadAsStringAsync();
            // Console.WriteLine(json);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("i", json);
        }
        
        [Fact]
        public async Task DeleteUser_unauthorized_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("x-test-UserId", "other");
            var response = await client.DeleteAsync("/user/delete/user1");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

                
        [Fact]
        public async Task DeleteUser_Authorized_ReturnsOk()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("x-test-UserId", "user1");
            var response = await client.DeleteAsync("/user/delete/user1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockUserService.Verify(s => s.DeleteAsync("user1"), Times.Once);
        }
    }
}