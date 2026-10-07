namespace hoslog.customer.shared.Multitenant.ClubInformationMultitenantCommon
{
    public class ClubReviewsMultitenantCommon : CustomerInfoMultitenantCommon
    {
        public List<ClubReviewHostMultitenantCommon>? getClubReviewHostList { get; set; } = new List<ClubReviewHostMultitenantCommon>();
        public List<ClubReviewRemarkMultitenantCommon>? getClubReviewRemarkList { get; set; } = new List<ClubReviewRemarkMultitenantCommon>();
        public List<IntentRemarksMultitenant>? getIntentRemarks { get; set; } = new List<IntentRemarksMultitenant>();
        public List<QNARemarksMultitenant>? qnaRemarks { get; set; } = new List<QNARemarksMultitenant>();
    }

    public class CustomerInfoMultitenantCommon
    {
        public string? reviewId { get; set; }
        public string? clubId { get; set; }
        public string? customerName { get; set; }
        public string? customerImage { get; set; }
        public string? ratingScale { get; set; }
        public long? commentedOn { get; set; }
        public long? visitedDate { get; set; }
        public bool isCourseExtended { get; set; }
        public bool didMetFavourite { get; set; }
        public int? intent { get; set; }
        public int? totalRecords { get; set; }
        public int? pageNo { get; set; }
        public int? recordPerPage { get; set; }
        public ImageResponse? customerImageResponse { get; set; } = new ImageResponse();
    }
    public class ClubReviewHostMultitenantCommon
    {
        public string? hostName { get; set; }
        public string? hostImage { get; set; }
        public string? hostId { get; set; }
        public string? hostCode { get; set; }
        public bool? isMVPHost { get; set; }
        public ImageResponse? hostImageResponse { get; set; } = new ImageResponse();
    }

    public class ClubReviewRemarkMultitenantCommon
    {
        public string? englishRemark { get; set; }
        public string? japaneseRemark { get; set; }
        public string? remarkType { get; set; }
    }
    public class IntentRemarksMultitenant
    {
        public string? intentEnglishRemark { get; set; }
        public string? intentJapaneseRemark { get; set; }
        public string? intentRemarkType { get; set; }
    }

    public class QNARemarksMultitenant
    {
        public string? qnaEngQuestion { get; set; }
        public string? qnaJpQuestion { get; set; }
        public string? qnaJpShortQuestion { get; set; }
        public string? qnaEngAnswer { get; set; }
        public string? qnaJpAnswer { get; set; }
    }
}