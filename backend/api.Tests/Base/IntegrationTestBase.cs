using backend.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;

namespace api.Tests.Base
{
    public class IntegrationTestBase: IClassFixture<ApiTestFixTure>
    {
        protected readonly ApiTestFixTure _fixture;
        protected readonly WebApplicationFactory<Program> _factory;

        protected Mock<IRealtimeNotificationClient> _mockNotificatonClient => _fixture.MockNotificationClient;
        protected Mock<IUserService> _mockUserService => _fixture.MockUserService;
        protected Mock<IPostService> _mockPostService => _fixture.MockPostService;
        protected Mock<INotificationService> _mockNotificatonService => _fixture.MockNotificationService;
        protected Mock<NotificationService> _mockNotificatonServiceConcrete => _fixture.MockNotificationServiceConcrete;
        
        public IntegrationTestBase(ApiTestFixTure fixture)
        {
            _fixture = fixture;
            _factory = fixture;

            _mockNotificatonClient.Reset();
            _mockUserService.Reset();
            _mockPostService.Reset();
            _mockNotificatonService.Reset();
            _mockNotificatonServiceConcrete.Reset();
        }
    }
}