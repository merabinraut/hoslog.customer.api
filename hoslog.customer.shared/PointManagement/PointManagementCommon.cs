namespace hoslog.customer.shared.PointManagement
{
    public class PointManagementRequestCommon
    {
        public string? agentId { get; set; }
        public string? transactionMode { get; set; }
        public int? row { get; set; } = 0;
        public int? fetch { get; set; } = 10;
    }

    public class PointManagementResponseCommon : CommonDBResponse
    {
        public long? transactionDate { get; set; } = null!;
        public string transactionDateType { get; set; } = null!;
        public string transactionMode { get; set; } = null!;
        public int point { get; set; }
        public string remark { get; set; } = null!;
        public int totalPoint { get; set; }
        public int? progressCount { get; set; }
        public string? storeName { get; set; }
        public string? storeNameJp { get; set; }
        public int totalData { get; set; }
    }
}