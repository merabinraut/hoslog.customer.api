using hoslog.customer.shared;

namespace hoslog.customer.common.Models
{
    public class PhotoGalleryRequestCommon
    {
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public int pageNo { get; set; }
    }
    public class PhotoGalleryResponseCommon : CommonApiResponse
    {
        public string? imagePath { get; set; }
        public string? imageTitle { get; set; }
        public int? pageNo { get; set; } = 0;
        public int? totalRecords { get; set; } = 0;
        public int? recordPerPage { get; set; } = 10;
    }

    public class PhotoGalleryResponseModel
    {
        public PaginationResponse pagination { get; set; } = new PaginationResponse();
        public List<ImageResponse> images { get; set; } = new List<ImageResponse>();
    }
}
