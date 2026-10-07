using System;
using System.Collections.Generic;
using System.Text;
using hoslog.customer.common.Models;
using hoslog.customer.shared;

namespace hoslog.customer.common.RealTime.Models.Notification
{
    public sealed class NotificationRefreshPayload
    {
        public string? userId { get; set; }
        public NotificationDetailCommon notification { get; set; } = new();

        public string refreshReason { get; set; } = "NotificationCreated";
    }
   
    
}
