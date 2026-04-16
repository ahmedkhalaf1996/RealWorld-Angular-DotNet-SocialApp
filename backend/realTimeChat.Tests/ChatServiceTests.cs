
using realTimeServices.Services;
using StackExchange.Redis;
using Moq;
using Xunit;

namespace realTimeChat.Tests
{
    public class ChatServiceTests
    {
        private Mock<IDatabase> _mockDb;
        private Mock<IConnectionMultiplexer> _mockRedis;

        public ChatServiceTests()
        {
            _mockDb = new Mock<IDatabase>();
            _mockRedis = new Mock<IConnectionMultiplexer>();
            _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDb.Object);
        }

        [Fact]
        public void AddUseronnId_thenGetConntionidByUser_ReturnsConnit()
        {
            _mockDb.Setup(db => db.HashGet("OnlineUsers", "user1", CommandFlags.None))
                   .Returns("conn1");
            
            var service = new ChatService(_mockRedis.Object, null);
            service.AddUserConnectionId("user1", "conn1");

            var connectionId = service.GetConnectionIdByUser("user1");

            Assert.Equal("conn1", connectionId);
        }

        [Fact]
        public void GetUserIdByConntionIt_ReturnsUserId()
        {
            _mockDb.Setup(db => db.HashGetAll("OnlineUsers", CommandFlags.None))
                   .Returns(new HashEntry[] { new HashEntry("user2", "conn2") });
                   
            var service = new ChatService(_mockRedis.Object, null);
            service.AddUserConnectionId("user2", "conn2");

            var userId = service.GetUserIdByConnectionID("conn2");

            Assert.Equal("user2", userId);
        }

        [Fact]
        public void RemoveUserFromList_InvalidUser_ReturnsNull()
        {
            var service = new ChatService(_mockRedis.Object, null);
            var result = service.RemoveUserFromList("");
            Assert.Null(result);        
        }

        [Fact]
        public void GetOnlineUsers_WhneNoRoom_ReturnsEmptyArray()
        {
            _mockDb.Setup(db => db.HashGet("UserRooms", "missing", CommandFlags.None))
                   .Returns(RedisValue.Null);

            var service = new ChatService(_mockRedis.Object, null);
            var result = service.GetOnlineUsers("missing");

            Assert.Empty(result); 
        }
    }
}


// using realTimeServices.Services;

// namespace realTimeChat.Tests
// {
//     public class ChatServiceTests
//     {
//         [Fact]
//         public void AddUseronnId_thenGetConntionidByUser_ReturnsConnit()
//         {
//             var service = new ChatService();
//             service.AddUserConnectionId("user1", "conn1");
//             //
//             var connectionId = service.GetConnectionIdByUser("user1");

//             Assert.Equal("conn1", connectionId);
//         }

//         [Fact]
//         public void GetUserIdByConntionIt_ReturnsUserId()
//         {
//             var service = new ChatService();
//             service.AddUserConnectionId("user2", "conn2");
//             //
//             var userId = service.GetUserIdByConnectionID("conn2");

//             Assert.Equal("user2", userId);
//         }

//         [Fact]
//         public void RemoveUserFromList_InvalidUser_ReturnsNull()
//         {
//             var service = new ChatService();
//             var result = service.RemoveUserFromList("");
//             Assert.Null(result);        
//         }

//         [Fact]
//         public void GetOnlineUsers_WhneNoRoom_ReturnsNull()
//         {
//             var service = new ChatService();
//             var result = service.GetOnlineUsers("missing");

//             Assert.Null(result);
//         }
//     }
// }