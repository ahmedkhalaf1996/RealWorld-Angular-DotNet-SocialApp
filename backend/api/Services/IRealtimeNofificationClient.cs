using backend.Protos;

namespace backend.Services
{
    public interface IRealtimeNotificationClient
    {
        Task SendGrpcNotificationAsync(NotificationGrpcRequest request);
    }
}