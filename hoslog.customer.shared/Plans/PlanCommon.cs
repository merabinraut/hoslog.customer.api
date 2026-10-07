namespace hoslog.customer.shared.Plans
{
    public class PlanCommon
    {
        public string? id { get; set; }
        public string? planName { get; set; }
        public string? planType { get; set; }
        public string? remarks { get; set; }
        public string? isActive { get; set; }
        public string? strikePrice { get; set; }
        public string? isPopularity { get; set; }
        public string? isStrikeOut { get; set; }
        public string? price { get; set; }
        public string? planTime { get; set; }
        public string? liquor { get; set; }
        public string? nomination { get; set; }
        public bool hasMostPopularTag { get; set; }
        public string? planNameEnglish { get; set; }
    }

    public class PlanCommonResponse
    {
        public string? id { get; set; }
        public string? planName { get; set; }
        public string? planType { get; set; }
        public string? remarks { get; set; }
        public Boolean? isActive { get; set; }
        public int? strikePrice { get; set; }
        public Boolean? isPopularity { get; set; }
        public Boolean? isStrikeOut { get; set; }
        public int? price { get; set; }
        public string? planTime { get; set; }
        public string? liquor { get; set; }
        public int? nomination { get; set; }
        public bool hasMostPopularTag { get; set; }
        public string? planNameEnglish { get; set; }
    }

    public class ClubPlanResponseCommon : CommonApiResponse
    {
        public int? planId { get; set; }
        public int? clubId { get; set; }
        public string? planName { get; set; }
        public string? planTime { get; set; }
        public string? planPrice { get; set; }
        public string? noOfPeople { get; set; }
        public string? noOfNomination { get; set; }
    }

    public class ClubPlanListRequestCommon
    {
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public int pageNo { get; set; } = 1;
    }

    public class ClubPlanListResponseCommon
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
        public int totalRecords { get; set; }
        public int pageNo { get; set; }
        public int recordPerPage { get; set; }
        public bool hasMostPopularTag { get; set; }
        public string? planNameEnglish { get; set; }
    }
}
