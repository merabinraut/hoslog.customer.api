namespace hoslog.customer.shared.LocationManagement
{
    public class LocationCommon
    {
        public string? id { get; set; }
        public string? locationName { get; set; }
        public string? imagePath { get; set; }
        public string? isActive { get; set; }
        public string? clubCount { get; set; }
        public string? hostCount { get; set; }
        public string? locationURL { get; set; }
        public string? subLocationName { get; set; }
        public string? subLocationNameEng { get; set; }
        public string? longitude { get; set; }
        public string? latitude { get; set; }
        public string? prefecture { get; set; }
    }

    public class LocationCommonResponse
    {
        public string? id { get; set; }
        public string? locationName { get; set; }
        public string? subLocationName { get; set; }
        public string? subLocationNameEng { get; set; }
        public string? prefecture { get; set; }
        public Boolean? isActive { get; set; }
        public int? clubCount { get; set; }
        public int? hostCount { get; set; }
        public ImageResponse? image { get; set; }
        public LocationResponse? location { get; set; }
    }
}
