using System;
using System.Collections.Generic;
using System.Text;

namespace hoslog.customer.common.RealTime.Models.RecommendationGroup
{
    public class RecommendationRefreshPayload
    {
        public string? locationId { get; set; }
        public string refreshReason { get; set; } = "PositionChanged";
        public DateTime generatedUTCDate { get; set; } = DateTime.UtcNow;
    }
}
