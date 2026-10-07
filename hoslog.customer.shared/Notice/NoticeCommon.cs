namespace hoslog.customer.shared.Notice
{
    public class NoticeDetailResponseCommon : CommonApiResponse
    {
        public string? eventTitle { get; set; }
        public string? eventDescription { get; set; }
        public string? eventType { get; set; }
        public string? eventStatus { get; set; }
        public long? eventDateTime { get; set; }

    }
    public class NoticeDetailResponse
    {
        public string? eventTitle { get; set; }
        public string? eventDescription { get; set; }
        public string? eventType { get; set; }
        public string? eventStatus { get; set; }
        public long? eventDateTime { get; set; }
    }
}
