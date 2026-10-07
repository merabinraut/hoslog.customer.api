namespace hoslog.customer.shared.ReferralManagement
{
    public class ReferralPointInfoCommon
    {
        public string stageCode { get; set; }
        public string reservationCount { get; set; }
        public string points { get; set; }
    }


    public class ReferralPointTotalCommon
    {
        public int referralPoints { get; set; }
        public int reviewPoints { get; set; }
        public int totalReferralPoints { get; set; }
    }


    public class ReferralInfoStatusCommon
    {
        public int VisitedFriendsCount { get; set; }
        public int MonthlyProgressCount { get; set; }
        public int MonthlyProgressTarget { get; set; }
        public int RemainingVisitsForNextBonus { get; set; }
        public long NextUpdateDate { get; set; }
    }

}
