namespace hoslog.customer.api.Models
{
    public class AmazonS3Model
    {
        public string? BucketName { get; set; } = null;
        public string? ConfigAccess { get; set; } = null;
        public string? ConfigSecret { get; set; } = null;
        public string? BaseUrl { get; set; } = null;
        public double TimeOutDuration { get; set; } = 0;
        public string? KeyName { get; set; } = null;
        public string? ImageVirtualPath { get; set; } = null;
        public string? NoImageURL { get; set; }= null;
    }
    public class AmazonConfiguration
    {
        public AmazonS3Model? AmazonS3Configuration { get; set; }
    }
    public enum AWSBucketFolderNameModel
    {
        ADMIN,
        CLUB,
        CUSTOMER,
        AFFILIATE,
        HOST,
        DOCUMENTS
    }
    
}
