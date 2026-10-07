using hoslog.customer.shared;
using hoslog.customer.shared.Notification;

namespace hoslog.customer.domain.Notification
{
    public interface INotificationRepository
    {
        Task<List<NotificationDetailResponseCommon>> GetNotification(CommonDBRequest request, int pageNo);
        Task<NotificationMarkAllAsReadResponseCommon> MarkAllAsReadNotificationAsync(string userId, string actionIP, string actionPlatform);
        Task<CommonDBResponse> MarkAsReadNotificationAsync(NotificationMarkAsReadRequestCommon request);
        Task<UnreadNotificationStatusCommon> CheckUnreadNotification(CommonDBRequest request);
    }
}
