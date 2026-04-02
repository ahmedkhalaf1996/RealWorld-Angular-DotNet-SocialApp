
using backend.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace api.Tests.Base
{
    public class ApiTestFixTure: WebApplicationFactory<Program>
    {
        public Mock<IRealtimeNotificationClient> MockNotificationClient {get;} = new();
        public Mock<IUserService> MockUserService {get;} = new();
        public Mock<IPostService> MockPostService {get;} = new();
        public Mock<INotificationService> MockNotificationService {get;} = new();

        public Mock<NotificationService> MockNotificationServiceConcrete {get;} = new(MockBehavior.Strict,
        
            Microsoft.Extensions.Options.Options.Create(new backend.Models.MongoDBSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "testdb",
                NotificationCollection = "notifications"
            }));

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
             // todo Defult test Auth
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            }).AddScheme<TestAuthHandlerOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, _ => {}
            );
            // replace real services with mocks
             services.RemoveAll<IUserService>();
             services.AddSingleton<IUserService>(_ => MockUserService.Object);

             services.RemoveAll<IPostService>();
             services.AddSingleton<IPostService>(_ => MockPostService.Object);

            services.RemoveAll<INotificationService>();
             services.AddSingleton<INotificationService>(_ => MockNotificationService.Object);


             services.RemoveAll<NotificationService>();
             services.AddSingleton<NotificationService>(_ => MockNotificationServiceConcrete.Object);  

             services.RemoveAll<IRealtimeNotificationClient>();
             services.AddSingleton<IRealtimeNotificationClient>(_ => MockNotificationClient.Object);

            });
            
        }

    }
}