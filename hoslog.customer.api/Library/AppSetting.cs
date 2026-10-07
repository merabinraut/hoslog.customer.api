using hoslog.customer.api.Models;

namespace hoslog.customer.api.Library
{
    public class AppSettingData
    {
        private IConfigurationRoot AllData;
        public string envType = string.Empty;
        public AppSettingData()
        {

            var lconfigurationBuilder = new ConfigurationBuilder();
            var lpath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            lconfigurationBuilder.AddJsonFile(lpath, false);
            IConfigurationRoot localdata = lconfigurationBuilder.Build();


            string env = localdata.GetSection("environment").Value;
            envType = env;

            //evivorment files
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            AllData = configurationBuilder.Build();
            //amazon s3 model
        }

        public AmazonS3Model GetAmazonSetting
        {
            get
            {
                var amazonSettings = new AmazonS3Model
                {
                    ConfigAccess = AllData.GetSection("ConfigAccess").Value,
                    ConfigSecret = AllData.GetSection("ConfigSecret").Value,
                    BucketName = AllData.GetSection("BucketName").Value,
                    BaseUrl = AllData.GetSection("BaseUrl").Value,
                    ImageVirtualPath = AllData.GetSection("ImageVirtualPath").Value,
                    TimeOutDuration = double.Parse(AllData.GetSection("TimeOutDuration").Value),
                };
                return amazonSettings;
            }
        }

        public string PreviousImageUrl
        {
            get
            {
                return AllData.GetSection("PreviousImageUrl").Value;
            }
        }

        public string BaseUrl
        {
            get
            {
                return AllData.GetSection("BaseUrl").Value;
            }
        }

        public string BucketName
        {
            get
            {
                return AllData.GetSection("BucketName").Value;
            }
        }
        public string ConfigAccess
        {
            get
            {
                return AllData.GetSection("ConfigAccess").Value;
            }
        }
        public string ConfigSecret
        {
            get
            {
                return AllData.GetSection("ConfigSecret").Value;
            }
        }
        public double TimeOutDuration
        {
            get
            {
                return double.Parse(AllData.GetSection("TimeOutDuration").Value);
            }
        }
        public IConfigurationSection GetIpRateLimit
        {
            get
            {
                return AllData.GetSection("IpRateLimiting");
            }
        }
    }
}
