

using System.Net;
using System.Net.Http.Json;
using api.Tests.Base;
using backend.Models;
using backend.Protos;
using Moq;

namespace api.Tests.Controllers
{
    public class PostControllerTests : IntegrationTestBase
    {
        public PostControllerTests(ApiTestFixTure fixture) : base (fixture){}

        [Fact]
        public async Task CreatePost_ValidData_reaturnsCreatedPost()
        {
            var userId = "user1";
            var client = _factory.CreateClient();

            client.DefaultRequestHeaders.Add("x-test-UserId", userId);

            var newPost = new Post {title = "Test Title", message = "Test Message", selectedFile = "base64data"};
            _mockPostService.Setup(s => s.CreateOnePostAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            var response = await client.PostAsJsonAsync("/posts", newPost);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockPostService.Verify(s => s.CreateOnePostAsync(
                It.Is<Post>(p => p.title == newPost.title && p.creator == userId)), Times.Once);
        }

        [Fact]
        public async Task LikePost_TrigersNotification()
        {
            var likerId = "liker";
            var creatorId = "creator";
            var postId = "post123";

            var post = new Post { _id = postId, creator = creatorId, likes = new List<string>()};
            var likerUser = new User {_id = likerId, name = "liker Name"};

            _mockPostService.Setup(s => s.GetPostByID(postId)).ReturnsAsync(new PostResponse {_id = postId, creator = creatorId, likes = new List<string>()});
            _mockPostService.Setup(s => s.GetUsByid(likerId)).ReturnsAsync(likerUser);
            _mockPostService.Setup(s => s.UpdatePost(postId, It.IsAny<Post>())).ReturnsAsync(new Post());

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("x-test-UserId", likerId);

            //
            var response = await client.PatchAsync($"/posts/{postId}/likePost", null);

            // assert
            if(response.StatusCode != HttpStatusCode.OK)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Xunit.Sdk.XunitException($"Expected Ok but got {response.StatusCode}: Body : {error}");
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // verifiy gRPC notification was sent
            _mockNotificatonClient.Verify(c => c.SendGrpcNotificationAsync(It.IsAny<NotificationGrpcRequest>()), Times.Once);
        }
    }
}