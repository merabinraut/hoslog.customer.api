using System;
using System.Collections.Generic;
using System.Text;

namespace hoslog.customer.common.Models.Referral
{


    public class ReferralPointInfoResponse
    {
        public List<ReferralPointInfoModel> stage1 { get; set; }
        public ReferralPointInfoModel stage2 { get; set; }
    }
    public class ReferralPointInfoModel
    {
        public string stageCode { get; set; }
        public string reservationCount { get; set; }
        public string points { get; set; }

    }

    public class ReferralPointTotalModel
    {
        public int referralPoints { get; set; }
        public int reviewPoints { get; set; }
        public int totalReferralPoints { get; set; }
    }

}
