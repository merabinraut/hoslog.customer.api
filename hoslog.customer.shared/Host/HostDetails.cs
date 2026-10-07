namespace hoslog.customer.shared.Host
{
    public class HostDetails
    {

    }
    public class ScheduleDetailsRequestCommon : CommonDBRequest
    {
        public long? date { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
    }

    public class HostListRequestCommon
    {
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public string? userId { get; set; }
        public int pageNo { get; set; }
        public int pageSize { get; set; }
    }

    public class HostListResponseCommon : CommonDBResponse
    {
        public string? clubId { get; set; }
        public string? hostId { get; set; }
        public string? hostCode { get; set; }
        public string? hostName { get; set; }
        public string? hostNameJapanese { get; set; }
        public int rank { get; set; }
        public bool isBookMarked { get; set; }
        public string? hostImage { get; set; }
        public string? position { get; set; }
        public string? hostTitle { get; set; }
        public int? totalRecords { get; set; }
        public int? pageNo { get; set; }
        public int? recordPerPage { get; set; }
    }
}
