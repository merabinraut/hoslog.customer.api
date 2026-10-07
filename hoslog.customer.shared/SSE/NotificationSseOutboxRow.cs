using System;
using System.Collections.Generic;
using System.Text;

namespace hoslog.customer.shared.SSE
{
    public sealed class NotificationSseOutboxRow
    {
        public long outboxId { get; set; }
        public long notificationId { get; set; }
        public long agentId { get; set; }
        public int roleType { get; set; }
        public int attemptCount { get; set; }
        public string? userId { get; set; }

        public long notificationTo { get; set; }
        public string? notificationType { get; set; }
        public string? notificationSubject { get; set; }
        public string? notificationCategory { get; set; }
        public string? notificationBody { get; set; }
        public string? notificationStatus { get; set; }
        public string? notificationReadStatus { get; set; }
        public int allNotificationCount { get; set; }
        public int unReadNotificationCount { get; set; }

        public string? dateCategory { get; set; }
        public string? formattedDate { get; set; }
        public string? notificationURL { get; set; }
        public long? createdDate { get; set; }
        public string? additionalDetail1 { get; set; }
        public string? additionalDetail2 { get; set; }
        public string? additionalDetail3 { get; set; }

        public string? notificationImage { get; set; }


    }
}
