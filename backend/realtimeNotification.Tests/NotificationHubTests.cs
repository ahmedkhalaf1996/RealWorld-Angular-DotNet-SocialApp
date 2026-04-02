
using Microsoft.AspNetCore.SignalR;
using RealTimeNotification.Hubs;
using Moq;
namespace RealTimeNotification.Tests
{
    public class NotificationHubTests
    {
        [Fact]
        public async Task JoinChannel_AddsToGroup()
        {
            var hub = new NotificationHub();
            var mockGroups = new Mock<IGroupManager>();
            var mockContext = new Mock<HubCallerContext>();

            hub.Context = mockContext.Object;
            hub.Groups = mockGroups.Object;

            mockContext.SetupGet(c => c.ConnectionId).Returns("conn-1");

            await hub.JoinChannel("user1");

            mockGroups.Verify(g => g.AddToGroupAsync("conn-1", "user1", default), Times.Once);
        }

        [Fact]
       public async Task SendNotification_SendsToGroup()
        {
            var hub = new NotificationHub();
            var mockClients = new Mock<IHubCallerClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            hub.Clients = mockClients.Object;

            mockClients.Setup(c => c.Group("user1")).Returns(mockClientProxy.Object);

            await hub.SendNotification("user1", "hello");

            mockClientProxy.Verify(c => c.SendCoreAsync("ReceiveNotification", It.Is<object[]>(a => a.Length == 1), default), Times.Once);
        
        }
    }
}