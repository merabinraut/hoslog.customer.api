namespace hoslog.customer.common.Models.PointManagement
{
    public class PointManagementResponseModel
    {
        public long transactionDate { get; set; }
        public string transactionDateType { get; set; } = null!;
        public string transactionMode { get; set; } = null!;
        public int point { get; set; }
        public string remark { get; set; } = null!;
        public int totalPoint { get; set; }
        public int? progressCount { get; set; }
        public string? storeName { get; set; }
        public string? storeNameJp { get; set; }
    }
}