

using backend.Protos;
using Grpc.Net.Client;

namespace backend.Services
{
    public class RealtimeNotificationClient : IRealtimeNotificationClient
    {

        private readonly ILogger<RealtimeNotificationClient> _logger;

        private readonly NotificationGrpcService.NotificationGrpcServiceClient _client;

        public RealtimeNotificationClient(ILogger<RealtimeNotificationClient> logger, IConfiguration config)
        {
            _logger = logger;
            // ger url form conf or env 
            var serviceUrl =config["GrpcServices:NotificationService"] 
                ?? Environment.GetEnvironmentVariable("NOTIFICATION_SERVVICE_URL") 
                ?? "http://localhost:8090";
            var channel = GrpcChannel.ForAddress(serviceUrl);
            _client = new NotificationGrpcService.NotificationGrpcServiceClient(channel);
        }


        public async Task SendGrpcNotificationAsync(NotificationGrpcRequest request)
        {
            
            try
            {
                await _client.SendGrpcNotificationAsync(request);
                _logger.LogInformation($"Notification sent via gRPC to user {request.Mainuid}");
            }
            catch (Exception ex)
            {
                
               _logger.LogError($"Faild to send notification via grpc: {ex.Message}");
               throw;
            }
        }
    }
}