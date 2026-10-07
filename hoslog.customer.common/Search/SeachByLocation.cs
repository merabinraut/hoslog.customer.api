using hoslog.customer.shared;

namespace hoslog.customer.common.Search
{
    public class SearchByLocationModel : CommonDBRequest
    {
        public string? locationName { get; set; }
        public string? subLocationName { get; set; }
        public string? subLocationEng { get; set; }
        public int pageNo { get; set; }
        public string? pageType { get; set; }
    }
    public class SearchByPreference
    {
        public string? searchByPreference { get; set; }
        public string? locationId { get; set; }
        public string? fee { get; set; }
        public string? searchByBusinessHour { get; set; }
        public string? shop { get; set; }
        public string? customerId { get; set; }
        public int pageNo { get; set; }
    }
    public class SearchByParticular
    {
        public string? SearchFilter { get; set; }
        public string? locationId { get; set; }
        public string? clubCategory { get; set; }
        public string? price { get; set; }
        public string? shift { get; set; }
        public string? clubAvailability { get; set; }
        public string? customerId { get; set; }
        public string? time { get; set; }
        public int pageNo { get; set; }
    }
    public class SearchByHostDetail
    {
        public string? searchByPreference { get; set; }
        public string? searchByHeight { get; set; }
        public string? searchByAge { get; set; }
        public string? searchByBloodType { get; set; }
        public string? searchByConstellation { get; set; }
        public string? locationId { get; set; }
        public int pageNo { get; set; }
    }
    public class SearchByLocationResponseModel
    {
        public string? locationUrl { get; set; }
    }
}
