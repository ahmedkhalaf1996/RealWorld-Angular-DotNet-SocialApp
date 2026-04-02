using System.Net;
using api.Tests.Base;
using backend.Models;
using Moq;

namespace api.Tests.Controllers
{
    public class NotificationControllerTests : IntegrationTestBase
    {
        public NotificationControllerTests(ApiTestFixTure fixture) : base(fixture){}

        [Fact]
        public async Task GetUserNotification_ReturnsOkWithNotifications()
        {
            var userId = "user123";
            var notifications = new List<Notification>
            {
                new Notification {mainuid = userId, deatils = "test"}
            };

            _mockNotificatonService
                .Setup(s => s.GetUserNotification(userId))
                .ReturnsAsync(notifications);
            
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/notification/{userId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task MarkNotificationAsReaded_MissingId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/notification/mark-notification-asreaded?id=");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task MarkNotificationAsReaded_WhenServiceReturnsFalse_ReturnsBadRequest()
        {
            var userId = "user123";
            _mockNotificatonServiceConcrete
                .Setup(s => s.MarkNotificationsAsReaded(userId))
                .ReturnsAsync(false);
            
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/notification/mark-notification-asreaded?id={userId}");
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        }


        [Fact]
        public async Task MarkNotificationAsReaded_wehnServiceReturnsTrue_ReturnOk()
        {
            var userId = "user123";
            var notification = new List<Notification>
            {
                new Notification {mainuid = userId, deatils = "test"}
            };

            _mockNotificatonService
                .Setup(s => s.MarkNotificationsAsReaded(userId))
                .ReturnsAsync(true);
            _mockNotificatonService
                .Setup(s => s.GetUserNotification(userId))
                .ReturnsAsync(notification);

            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/notification/mark-notification-asreaded?id={userId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}