namespace hoslog.customer.shared.ClubManagement
{
    public class ClubDetailCommon : PaginationResponse
    {
        public string? clubAgentId { get; set; }
        public string? clubNameEng { get; set; }
        public string? clubName { get; set; }
    }

    public class ClubDetailResponseCommon
    {
        public PaginationResponse pagination { get; set; } = new PaginationResponse();
        public List<ClubDetailCommonData> clubList { get; set; } = new List<ClubDetailCommonData>();
    }

    public class ClubDetailCommonData
    {
        public string? clubAgentId { get; set; }
        public string? clubNameEng { get; set; }
        public string? clubName { get; set; }
    }

    public class ClubsYouCanVisitNowRequestCommon : CommonDBRequest
    {
        public int pageNo { get; set; }
        public string? locationId { get; set; }
    }

    public class ClubsYouCanVisitNowResponseCommon : PaginationResponse
    {
        public string? clubId { get; set; }
        public string? clubName { get; set; }
        public string? clubNameJapanese { get; set; }
        public string? clubCode { get; set; }
        public string? locationId { get; set; }
        public string? clubLogo { get; set; }
        public string? clubLogoDescription { get; set; }
        public string? clubLogoAlt { get; set; }
    }
}
