

using realTimeServices.Services;

namespace realTimeChat.Tests
{
    public class ChatServiceTests
    {
        [Fact]
        public void AddUseronnId_thenGetConntionidByUser_ReturnsConnit()
        {
            var service = new ChatService();
            service.AddUserConnectionId("user1", "conn1");
            //
            var connectionId = service.GetConnectionIdByUser("user1");

            Assert.Equal("conn1", connectionId);
        }

        [Fact]
        public void GetUserIdByConntionIt_ReturnsUserId()
        {
            var service = new ChatService();
            service.AddUserConnectionId("user2", "conn2");
            //
            var userId = service.GetUserIdByConnectionID("conn2");

            Assert.Equal("user2", userId);
        }

        [Fact]
        public void RemoveUserFromList_InvalidUser_ReturnsNull()
        {
            var service = new ChatService();
            var result = service.RemoveUserFromList("");
            Assert.Null(result);        
        }

        [Fact]
        public void GetOnlineUsers_WhneNoRoom_ReturnsNull()
        {
            var service = new ChatService();
            var result = service.GetOnlineUsers("missing");

            Assert.Null(result);
        }
    }
}