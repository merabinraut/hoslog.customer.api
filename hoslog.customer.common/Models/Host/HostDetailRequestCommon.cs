using hoslog.customer.shared;

namespace hoslog.customer.common.Host
{
    public class HostDetailRequestCommon
    {
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public string? userId { get; set; }
    }
    public class HostDetailResponseCommon : CommonApiResponse
    {
        public string? hostId { get; set; }
        public string? hostName { get; set; }
        public bool? isBookMarked { get; set; }
        public string? clubCode { get; set; }
        public string? rank { get; set; }
        public string? dob { get; set; }
        public string? hostNameJapanese { get; set; }
        public string? address { get; set; }
        public string? hostImage { get; set; }
        public string? clubId { get; set; }
        public string? clubName { get; set; }
        public string? otherPosition { get; set; }
        public string? hostTitle { get; set; }
        public string? hostCode { get; set; }
    }
    public class HostDetailResponse
    {
        public string? hostId { get; set; }
        public string? hostName { get; set; }
        public string? hostNameEng { get; set; }
        public bool? isBookMarked { get; set; }
        public string? otherPosition { get; set; }
        public string? clubId { get; set; }
        public string? clubName { get; set; }
        public string? clubCode { get; set; }
        public string? rank { get; set; }
        public string? hostTitle { get; set; }
        public string? hostCode { get; set; }
        public ImageResponse? image { get; set; } = new ImageResponse();

    }
    public class ScheduleRequestCommon
    {
        public string? date { get; set; }
        public string? clubId { get; set; }
    }
    public class ScheduleResponseCommon : CommonApiResponse
    {
        public string? scheduleId { get; set; }
        public string? scheduleTitle { get; set; }
        public string? scheduleDate { get; set; }
        public string? scheduleDescription { get; set; }
        public string? eventType { get; set; }
        public string? scheduleImage { get; set; }
        public string? closingDate { get; set; }
    }
    public class ScheduleResponse
    {
        public string? scheduleId { get; set; }
        public string? scheduleTitle { get; set; }
        public string? scheduleDate { get; set; }
        public string? scheduleDescription { get; set; }
        public string? eventType { get; set; }
        public string? closingDate { get; set; }
        public ImageResponse? image { get; set; } = new ImageResponse();
    }
    #region Host Details
    public class GetHostListDetailsCommon
    {
        public string? hostId { get; set; }
        public string? hostCode { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public string? clubNameEnglish { get; set; }
        public string? clubNameJapanese { get; set; }
        public string? location { get; set; }
        public string? locationUrl { get; set; }
        public string? clubLocationName { get; set; }
        public string? clubLogo { get; set; }
        public ImageResponse? clubLogoRes { get; set; } = new ImageResponse();
        public string? hostNameJapanese { get; set; }
        public string? hostNameEnglish { get; set; }
        public string? hostDob { get; set; }
        public string? hostAge { get; set; }
        public string? hostBirthPlace { get; set; }
        public string? hostRank { get; set; }
        public string? hostIntroduction { get; set; }
        public string? hostHeight { get; set; }
        public string? hostWebsiteLink { get; set; }
        public string? hostTiktokLink { get; set; }
        public string? hostTwitterLink { get; set; }
        public string? hostInstagramLink { get; set; }
        public string? hostFacebookLink { get; set; }
        public string? hostLine { get; set; }
        public string? hostConstellationGroup { get; set; }
        public string? hostPreviousOccupation { get; set; }
        public string? hostBloodType { get; set; }
        public string? hostLiquorStrength { get; set; }
        public bool? isBookmarked { get; set; }
        public string? hostLoveCount { get; set; }
        public string? customerHostCompatibility { get; set; }
        public string? position { get; set; }
        public string? title { get; set; }
        public List<Skills> skills { get; set; } = new List<Skills>();
        public List<Personality> personality { get; set; } = new List<Personality>();
        public List<ImageResponse> imageResponse { get; set; } = new List<ImageResponse>();
    }
    #region 
    public class Skills
    {
        public string? labelType { get; set; }
        public string? labelEnglish { get; set; }
        public string? labelJapanese { get; set; }
        public string? labelValue { get; set; }
    }
    public class Personality
    {
        public string? labelType { get; set; }
        public string? labelEnglish { get; set; }
        public string? labelJapanese { get; set; }
        public string? labelValue { get; set; }
    }
    public class HostGallaryImage
    {
        public string imagePath { get; set; }
        //public ImageResponse ImageResponse { get; set; }
    }
    #endregion
    #endregion

    public class HostsWithRankOrderingResponseModel
    {
        public PaginationResponse paginationResponse { get; set; } = new PaginationResponse();
        public List<HostsWithRankOrderingModel> hostList { get; set; } = new List<HostsWithRankOrderingModel>();
    }

    public class HostsWithRankOrderingModel
    {
        public string? clubId { get; set; }
        public string? hostId { get; set; }
        public string? hostCode { get; set; }
        public string? hostName { get; set; }
        public int rank { get; set; }
        public bool isBookMarked { get; set; }
        public string? position { get; set; }
        public string? hostTitle { get; set; }
        public ImageResponse? image { get; set; } = new ImageResponse();
    }
}
