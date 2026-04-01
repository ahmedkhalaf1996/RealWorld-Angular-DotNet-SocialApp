
using backend.Models;
using backend.Protos;
using backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace api.Tests.Services
{
    public class RealTimeChatGrpcServiceTests
    {
        [Fact]
        public async Task GetUserFollowingFollowers_ReturnsListIds()
        {
            var userId = "user1";
            var user = new User
            {
                _id = userId,
                followers = new List<string> {"a","b"},
                following = new List<string> {"b", "c"}
            };

            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetUserByID(userId)).ReturnsAsync(user);

            var options = Options.Create(new MongoDBSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "testdb",
                UserCollection = "users",
                MessageCollection = "testmessage",
                UnMessageCollection = "any"
            });
            var chatService = new ChatService(options);

            var logger = new Mock<ILogger<RealtimeChatGrpcService>>();

            var grpcService = new RealtimeChatGrpcService(chatService, userService.Object, logger.Object);

            var response = await grpcService.GetUserFollowingFollowers(new UserID {Userid = userId}, null);

            Assert.Single(response.UserIDsLists);
            var ids = response.UserIDsLists[0].UserIdsList;
            Assert.Contains("a", ids);
            Assert.Contains("b", ids);
            Assert.Contains("c", ids);
            Assert.Equal(3,  ids.Count);

        }
    }
}