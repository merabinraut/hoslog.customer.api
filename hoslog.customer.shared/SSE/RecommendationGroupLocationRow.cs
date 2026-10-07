using System;
using System.Collections.Generic;
using System.Text;

namespace hoslog.customer.shared.SSE
{
    public sealed class RecommendationGroupChangeRow
    {
        public long locationId { get; set; }

        public string eventType { get; set; } = string.Empty;
    }
}
