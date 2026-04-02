using System.Net;
using System.Net.Http.Json;
using api.Tests.Base;
using backend.interfaces;

namespace api.Tests.Controllers
{
    public class ChatControllerTests: IntegrationTestBase
    {
        public ChatControllerTests(ApiTestFixTure fixture): base(fixture)
        {
        }

        [Fact]
        public async Task SendMessage_MissingFields_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var body = new SendMessageInterface
            {
                content = "hi",
                sender = null,
                recever = "user2"
            };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            var response = await client.PostAsJsonAsync("/chat/sendmessage", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task GetUserMessageByNums_MissingQuery_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/chat/getmsgsbynums?from=&firstuid=&seconduid=");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
        }
        [Fact]
        public async Task GetUserUnreadedMessages_MissingUser_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/chat/get-user-unreadedmsg?userid=");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
        }
        [Fact]
        public async Task MarMessageAsReaded_MissingQuery_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/chat/mark-msg-asreaded?mainuid=&otheruid=");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
        }
    }
}