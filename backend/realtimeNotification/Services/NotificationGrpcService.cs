using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using RealTimeNotification.Protos;
using RealTimeNotification.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace RealTimeNotification.Services
{
    public class NotificationService : NotificationGrpcService.NotificationGrpcServiceBase
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService (
            ILogger<NotificationService> logger,
            IHubContext<NotificationHub> hubContext
        )
        {
            _logger = logger;
            _hubContext = hubContext;
        }
        

        public override async Task<Empty> SendGrpcNotification(
            NotificationGrpcRequest request,
            ServerCallContext context
        )
        {
            _logger.LogInformation($"grpc server: received notification form user id :{request.Mainuid}");
            _logger.LogInformation($"Grpc server: Deatils: {request.Deatils}");

            try
            {
                await _hubContext.Clients.Group(request.Mainuid).SendAsync("ReceiveNotification", new
                {
                    id = request.Id,
                    deatils = request.Deatils, 
                    targetid = request.Targetid,
                    mainuid = request.Mainuid,
                    isreded = request.Isreded,
                    createdAt = request.CreatedAt,
                    user = new
                    {
                        name = request.User?.Name,
                        imageUrl = request.User?.ImageUrl
                    }
                });
                _logger.LogInformation($"gRPC Server : Successfully broadcasted notificaton to group: {request.Mainuid}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"grpc server : fiald to broadcast notificaon: {ex.Message}");
            }

            return new Empty();
        }
    }
}



