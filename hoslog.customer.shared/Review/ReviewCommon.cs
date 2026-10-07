namespace hoslog.customer.shared.Review
{
    public class ReviewCommon : PaginationResponse
    {
        public string? clubNameEng { get; set; }
        public string? clubNameJap { get; set; }
        public long? commentedOn { get; set; }
        public string? ratingScale { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public string? clubLocationID { get; set; }
        public string? image { get; set; }
        public string? type { get; set; }
        public string? reservationId { get; set; }
    }

    public class ReviewCommonData
    {
        public PaginationResponse pagination { get; set; } = new PaginationResponse();
        public List<ReviewCommonResponse> reviewList { get; set; } = new List<ReviewCommonResponse>();
    }

    public class ReviewCommonResponse
    {
        public string? clubNameEng { get; set; }
        public string? clubName { get; set; }
        public long? commentedOn { get; set; }
        public int? ratingScale { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public string? clubLocationID { get; set; }
        public string? type { get; set; }
        public string? reservationId { get; set; }
        public ImageResponse? image { get; set; } = new ImageResponse();
    }
    #region  REVIEW MODEL STEP 1
    public class ReviewReservationRequestModel
    {
        public string? reservationId { get; set; }
        public string? userId { get; set; }
    }
    public class ReviewReservationResponseModel
    {
        public string? code { get; set; }
        public string? message { get; set; }
        public string? customerId { get; set; }
        public string? reservationId { get; set; }
        // public string? reservationDate { get; set; }
        // public string? reservationTime { get; set; }
        public string? price { get; set; }
        public string? planName { get; set; }
        public string? noOfPeople { get; set; }
        public string? usedPoint { get; set; }
        public string? totalAmount { get; set; }
        public string? paymentMethod { get; set; }
        public string? clubId { get; set; }
        public string? clubLogo { get; set; }
        public string? clubNameEnglish { get; set; }
        public string? clubNameJapanese { get; set; }
        public string? clubLocationName { get; set; }
        public string? clubLocationNameFull { get; set; }
        public long? reservationDateTime { get; set; }
        public string? landLineNumber { get; set; }
        public ImageResponse? clubLogoImageResp { get; set; } = new ImageResponse();
    }
    # endregion

    #region  REVIEW MODEL STEP 2
    public class ReviewStep2CommonModel
    {
        public IList<ReviewHostListModel>? getReviewHostList { get; set; }
        public IList<ReviewClubShortDetailModel>? getClubShortDetail { get; set; }
    }
    public class ReviewClubShortDetailModel
    {
        public string? clubId { get; set; }
        public string? clubName { get; set; }
        public string? clubNameJapanese { get; set; }
        public string? clubLogo { get; set; }
        public string? clubLocationName { get; set; }
        public ImageResponse? clubLogoResponse { get; set; } = new ImageResponse();

    }
    public class ReviewHostListModel
    {
        public string? clubId { get; set; }
        public string? hostId { get; set; }
        public string? hostName { get; set; }
        public string? hostNameJapanese { get; set; }
        public string? hostRank { get; set; }
        public string? hostPosition { get; set; }
        public string? hostImage { get; set; }
        public ImageResponse? hostImageResponse { get; set; } = new ImageResponse();
    }
    #endregion

    #region  REVIEW MODEL STEP 3
    public class ReviewStep3CommonModel
    {
        public IList<ReviewRemarkListModel>? getReviewRemarkList { get; set; }
        public IList<ReviewClubShortDetailModel>? getClubShortDetail { get; set; }
    }
    public class ReviewRemarkListModel
    {
        public string? remarkId { get; set; }
        public string? remarkLabelJapanese { get; set; }
        public string? remarkLabelEnglish { get; set; }
        public string? remarkTypeEnglish { get; set; }
        public string? remarkTypeJapanese { get; set; }
    }
    #endregion

    #region  REVIEW MODEL STEP 4
    public class ReviewStep4CommonModel
    {
        public IList<ReviewDichotomousQuestionModel>? getReviewDichotomousQuestion { get; set; }
        public IList<ReviewDichotomousAnswerModel>? getReviewDichotomousAnswer { get; set; }
        public IList<ReviewClubShortDetailModel>? getClubShortDetail { get; set; }
    }
    public class ReviewDichotomousQuestionModel
    {
        public string? remarkId { get; set; }

        public string? remarkLabelEnglish { get; set; }
        public string? remarkLabelJapanese { get; set; }
    }
    public class ReviewDichotomousAnswerModel
    {
        public string? remarkId { get; set; }
        public string? remarkLabelJapanese { get; set; }
        public string? remarkLabelEnglish { get; set; }
    }
    #endregion

    public class ReviewRequestModel
    {
        public string? userId { get; set; }
        public string? reservationId { get; set; }
        public string? clubId { get; set; }
        public string? hostList { get; set; }
        public string? mvpHostId { get; set; }
        public string? dichotomousList { get; set; }
        public string? remarkList { get; set; }
        public int ratingScale { get; set; }
        public int intent { get; set; }
        public string? remark { get; set; }
    }
}
