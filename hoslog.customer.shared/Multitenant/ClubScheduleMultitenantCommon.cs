namespace hoslog.customer.shared.Multitenant
{
    public class ClubScheduleMultitenantCommon
    {
        public long date { get; set; }
        public string englishDay { get; set; } = null!;
        public string japaneseDay { get; set; } = null!;
        public string scheduleLabel { get; set; } = null!;
        public string schedule { get; set; } = null!;
    }

    public class ScheduleDetailsRequestMultitenantCommon : CommonDBRequest
    {
        public string? subDomainName { get; set; }
        public long? date { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
    }

    public class ScheduleResponseMultitenantCommon : CommonApiResponse
    {
        public string? ScheduleId { get; set; }
        public string? ScheduleTitle { get; set; }
        public string? ScheduleDate { get; set; }
        public string? ScheduleDescription { get; set; }
        public string? EventType { get; set; }
        public string? ScheduleImage { get; set; }
        public string? closingDate { get; set; }
    }

    public class NoticeDetailRequestMultitenantCommon
    {
        public string? subDomainName { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public int? pageNo { get; set; }
    }

    public class NoticeDetailResponseMultitenantCommon : CommonApiResponse
    {
        public string? eventTitle { get; set; }
        public string? eventDescription { get; set; }
        public string? eventType { get; set; }
        public string? eventStatus { get; set; }
        public long? eventDateTime { get; set; }
    }

    #region Gallery
    public class PhotoGalleryRequestMultitenantCommon
    {
        public string? subDomainName { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
    }

    public class PhotoGalleryResponseMultitenantCommon : CommonApiResponse
    {
        public string? imagePath { get; set; }
        public string? imageTitle { get; set; }
        public int? pageNo { get; set; } = 0;
        public int? totalRecords { get; set; } = 0;
        public int? recordPerPage { get; set; } = 10;
    }
    #endregion
}