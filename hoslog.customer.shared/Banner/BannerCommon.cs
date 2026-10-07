namespace hoslog.customer.shared.Banner
{
    public class BannerCommon : CommonApiResponse
    {
        public string? BannerId { get; set; }
        public string? bannerTitle { get; set; }
        public string? bannerImage { get; set; }
        public string? bannerDescription { get; set; }
    }

    public class BannerResponse
    {
        public string? id { get; set; }
        public string? title { get; set; }
        public ImageResponse? image { get; set; } = new ImageResponse();
    }

}
