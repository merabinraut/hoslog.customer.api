using hoslog.customer.shared;

namespace hoslog.customer.common.Models
{
    public class NotificationMarkAsReadRequest
    {
        public string notificationId { get; set; } = null!;
    }
    public class NotificationDetailCommon
    {
        public string? notificationId { get; set; }
        public string? notificationTo { get; set; }
        public string? notificationType { get; set; }
        public string? notificationSubject { get; set; }
        public string? notificationBody { get; set; }
        public string? notificationStatus { get; set; }
        public string? notificationReadStatus { get; set; }
        public int allNotificationCount { get; set; }
        public string? dateCategory { get; set; }
        public string? formattedDate { get; set; }
        public string? notificationURL { get; set; }
        public int unReadNotificationCount { get; set; }
        public long? createdDate { get; set; }
        public string? additionalDetail1 { get; set; }
        public string? additionalDetail2 { get; set; }
        public string? additionalDetail3 { get; set; }
        public ImageResponse? image { get; set; } = new ImageResponse();
    }

    public class NotificationResponseModel
    {
        public PaginationResponse pagination { get; set; } = new PaginationResponse();
        public List<NotificationDetailCommon> notificationList { get; set; } = new List<NotificationDetailCommon>();
    }
}
