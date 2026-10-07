using Dapper;
using hoslog.customer.shared;
using hoslog.customer.shared.Notification;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Notification
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly RepositoryDao _dao;

        public NotificationRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<List<NotificationDetailResponseCommon>> GetNotification(CommonDBRequest request, int pageNo)
        {
            string proc = "apiproc_cp_customer_get_notification ";
            var parameters = new DynamicParameters();
            parameters.Add("@agentId", request.actionUserId);
            parameters.Add("@pageNo", pageNo);
            var dbResp = await _dao.ExecuteAsyncList<NotificationDetailResponseCommon>(proc, parameters);
            return dbResp;
        }

        public async Task<NotificationMarkAllAsReadResponseCommon> MarkAllAsReadNotificationAsync(string userId, string actionIP, string actionPlatform)
        {
            string proc = "apiproc_cp_mark_all_as_read_notification ";
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@actionIP", actionIP);
            parameters.Add("@actionPlatform", actionPlatform);
            var dbResp = await _dao.ExecuteAsync<NotificationMarkAllAsReadResponseCommon>(proc, parameters);
            return dbResp;
        }

        public async Task<CommonDBResponse> MarkAsReadNotificationAsync(NotificationMarkAsReadRequestCommon request)
        {
            string proc = "apiproc_cp_mark_as_read_notification ";
            var parameters = new DynamicParameters();
            parameters.Add("@userId", request.actionUserId);
            parameters.Add("@notificationId", request.notificationId);
            parameters.Add("@actionPlatform", request.actionPlatform);
            parameters.Add("@actionIP", request.actionIP);
            var dbResp = await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
            return dbResp;
        }

        public async Task<UnreadNotificationStatusCommon>CheckUnreadNotification(CommonDBRequest request)
        {
            string proc = "apiproc_cp_check_unread_notification ";
            var parameters = new DynamicParameters();
            parameters.Add("@agentId", request.actionUserId);
            var dbResp = await _dao.ExecuteAsync<UnreadNotificationStatusCommon>(proc, parameters);
            return dbResp;
        }
    }
}
