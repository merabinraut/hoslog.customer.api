namespace hoslog.customer.shared.Notification
{
    public class NotificationMarkAsReadRequestCommon : CommonDBRequest
    {
        public string? notificationId { get; set; }
    }
    public class NotificationDetailResponseCommon : CommonDBResponse, IPaginationResponse
    {
        public int? pageNo { get; set; }
        public int? totalRecords { get; set; }
        public int? recordPerPage { get; set; }
        public string? notificationId { get; set; }
        public string? notificationTo { get; set; }
        public string? notificationType { get; set; }
        public string? notificationSubject { get; set; }
        public string? notificationBody { get; set; }
        public string? notificationImageURL { get; set; }
        public string? notificationStatus { get; set; }
        public string? notificationReadStatus { get; set; }
        public int allNotificationCount { get; set; }
        public string? dateCategory { get; set; }
        public string? formattedDate { get; set; }
        public string? notificationImage { get; set; }
        public int unReadNotificationCount { get; set; }
        public long? createdDate { get; set; }
        public string? additionalDetail1 { get; set; }
        public string? additionalDetail2 { get; set; }
        public string? additionalDetail3 { get; set; }
    }
    public class NotificationMarkAllAsReadResponseCommon : CommonDBResponse
    {
        public string notificationIds { get; set; } = null!;
    }
    public class NotificationMarkAllAsReadResponse
    {
        public string notificaitonIds { get; set; } = null!;
    }

    public class UnreadNotificationStatusCommon : CommonDBResponse
    {
        public int? notificationUnreadStatus { get; set; }
    }

    public class UnreadNotificationStatusResponse
    {
        public Boolean? notificationUnreadStatus { get; set; }
    }
}
