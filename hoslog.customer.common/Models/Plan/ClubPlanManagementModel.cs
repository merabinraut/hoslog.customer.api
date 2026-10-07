
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.Plan
{
    public class ClubPlanManagementModel
    {
    }
    public class ClubPlanResponseModel
    {
        public string? planId { get; set; }
        public string? planName { get; set; }
        public string? planTime { get; set; }
        public string? planPrice { get; set; }
        public string? noOfPeople { get; set; }
        public string? noOfNomination { get; set; }
    }

    public class ClubAllPlanListResponseModel
    {
        public PaginationResponse pagination { get; set; } = new PaginationResponse();
        public List<ClubAllPlanListModel> plans { get; set; } = new List<ClubAllPlanListModel>();
    }

    public class ClubAllPlanListModel
    {
        public string? planId { get; set; }
        public string? planName { get; set; }
        public string? planTime { get; set; }
        public int planPrice { get; set; }
        public int strikePrice { get; set; }
        public bool isStrikeOut { get; set; }
        public string? planNomination { get; set; }
        public string? planRemarks { get; set; }
        public string? planLiquor { get; set; }
        public string? planLiquorJapanese { get; set; }
        public string? openHours { get; set; }
        public string? closeHours { get; set; }
        public string? planType { get; set; }
        public int noOfPeople { get; set; }
        public bool hasMostPopularTag { get; set; }
        public string? planNameEnglish { get; set; }
    }
}
