namespace hoslog.customer.shared
{
    public class AmazonSettingsCommon
    {
        public string BucketName { get; set; } = null;
        public string ConfigAccess { get; set; } = null;
        public string ConfigSecret { get; set; } = null;
        public string BaseUrl { get; set; } = null;
        public double TimeOutDuration { get; set; } = 0;
        public string KeyName { get; set; } = null;
        public string ImageVirtualPath { get; set; } = null;
        public string PreviousImageUrl { get; set; } = null;
    }
}
