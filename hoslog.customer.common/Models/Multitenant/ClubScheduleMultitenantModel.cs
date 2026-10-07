using hoslog.customer.shared;

namespace hoslog.customer.common.Models.Multitenant
{
    public class ClubScheduleMultitenantModel
    {
        public long date { get; set; }
        public string englishDay { get; set; } = null!;
        public string japaneseDay { get; set; } = null!;
        public string scheduleLabel { get; set; } = null!;
        public string schedule { get; set; } = null!;
    }

    public class ScheduleResponseMultitenantModel
    {
        public string? ScheduleId { get; set; }
        public string? ScheduleTitle { get; set; }
        public string? ScheduleDate { get; set; }
        public string? ScheduleDescription { get; set; }
        public string? EventType { get; set; }
        public string? ClosingDate { get; set; }
        public ImageResponse? image { get; set; } = new ImageResponse();
    }

    public class NoticeDetailResponseMultitenantModel
    {
        public string? eventTitle { get; set; }
        public string? eventDescription { get; set; }
        public string? eventType { get; set; }
        public string? eventStatus { get; set; }
        public long? eventDateTime { get; set; }
    }

    #region  Photo gallery

    public class PhotoGalleryResponseMultitenantModel
    {
        public PaginationResponse pagination { get; set; } = new PaginationResponse();
        public List<ImageResponse> images { get; set; } = new List<ImageResponse>();
    }
    #endregion
}