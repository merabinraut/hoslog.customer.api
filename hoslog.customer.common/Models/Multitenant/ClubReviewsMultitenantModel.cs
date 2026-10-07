using hoslog.customer.shared;

namespace hoslog.customer.common.Models.Multitenant
{
    public class ClubReviewsMultitenantModel : CustomerInfoMultitenantModel
    {
        public List<ClubReviewHostMultitenantModel>? getClubReviewHostList { get; set; } = new List<ClubReviewHostMultitenantModel>();
        public List<ClubReviewRemarkMultitenantModel>? getClubReviewRemarkList { get; set; } = new List<ClubReviewRemarkMultitenantModel>();
        public List<IntentRemarksMultitenantModel>? getIntentRemarks { get; set; } = new List<IntentRemarksMultitenantModel>();
        public List<QNARemarksMultitenantModel>? qnaRemarks { get; set; } = new List<QNARemarksMultitenantModel>();
    }

    public class CustomerInfoMultitenantModel
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
    public class ClubReviewHostMultitenantModel
    {
        public string? hostName { get; set; }
        public string? hostImage { get; set; }
        public string? hostId { get; set; }
        public string? hostCode { get; set; }
        public bool? isMVPHost { get; set; }
        public ImageResponse? hostImageResponse { get; set; } = new ImageResponse();
    }

    public class ClubReviewRemarkMultitenantModel
    {
        public string? englishRemark { get; set; }
        public string? japaneseRemark { get; set; }
        public string? remarkType { get; set; }
    }
    public class IntentRemarksMultitenantModel
    {
        public string? intentEnglishRemark { get; set; }
        public string? intentJapaneseRemark { get; set; }
        public string? intentRemarkType { get; set; }
    }

    public class QNARemarksMultitenantModel
    {
        public string? qnaEngQuestion { get; set; }
        public string? qnaJpQuestion { get; set; }
        public string? qnaJpShortQuestion { get; set; }
        public string? qnaEngAnswer { get; set; }
        public string? qnaJpAnswer { get; set; }
    }
}