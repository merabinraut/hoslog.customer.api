using hoslog.customer.shared;
using hoslog.customer.shared.Notification;
using System.Net;

namespace hoslog.customer.business.Notification
{
    public interface INotificationBusiness
    {
        Task<(HttpStatusCode, object)> GetNotificationList(int pageNo, AmazonSettingsCommon common);
        Task<(HttpStatusCode, object)> MarkAllAsReadNotification();
        Task<(HttpStatusCode, object)> MarkAsReadNotification(NotificationMarkAsReadRequestCommon request);

        Task<(HttpStatusCode, object)> CheckUnreadNotification();
    }
}
