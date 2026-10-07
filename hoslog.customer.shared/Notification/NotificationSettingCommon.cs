namespace hoslog.customer.shared.Notification
{
    public class NotificationSettingCommon
    {
        public long id { get; set; }
        public long eventTypeId { get; set; }
        public long roleType { get; set; }
        public long channelId { get; set; }
        public string? templateSubject { get; set; }
        public string? templateHeader { get; set; }
        public string? templateBody { get; set; }
        public string? templateFooter { get; set; }
        public string? senderAddress { get; set; }
        public bool isEnabled { get; set; }
        public bool sendNotification { get; set; }
        public int version { get; set; }
        public string? placeholders { get; set; }
        public string? createdBy { get; set; }
        public string? createdDate { get; set; }
        public string? createdUTCDate { get; set; }
        public string? createdIP { get; set; }
        public string? createdPlatform { get; set; }
        public string? updatedBy { get; set; }
        public string? updatedDate { get; set; }
        public string? updatedUTCDate { get; set; }
        public string? updatedIP { get; set; }
        public string? updatedPlatform { get; set; }
    }

    public class SystemNotificationCommon : CommonDBRequest
    {
        public long? roleType { get; set; }
        public long? notificationSettingId { get; set; }
        public string? subjectJson { get; set; }
        public string? messageJson { get; set; }
        public int? version { get; set; }
        public bool? isEnabled { get; set; }
    }

    public class NotificationQueueCommon : CommonDBRequest
    {
        public long? notificationSettingId { get; set; }
        public string? roleType { get; set; }
        public string? channelName { get; set; }
        public string? senderAddress { get; set; }
        public string? recipientsJson { get; set; }
        public string? subjectJson { get; set; }
        public string? messageJson { get; set; }
        public long? scheduledAt { get; set; }
        public int? version { get; set; }
    }
}