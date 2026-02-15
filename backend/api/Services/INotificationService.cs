using backend.Models;
namespace backend.Services
{
    public interface INotificationService
    {
        Task CreateNotification(Notification notification);
        Task<List<Notification>> GetUserNotification(string uid);

        Task<bool> MarkNotificationsAsReaded(string uid);
    }
}