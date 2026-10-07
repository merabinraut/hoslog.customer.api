using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.business.CommonHandler
{
    public static class ImageHandler
    {
        public static string ProcessedImage(AmazonSettingsCommon awsSettingCommon, string ImageUrl, bool useDefaultImage = false)
        {
            string response = $"{awsSettingCommon.BaseUrl}/{awsSettingCommon.BucketName}/ADMIN/no-image.jpeg";
            if (!useDefaultImage && !string.IsNullOrEmpty(ImageUrl) && ImageUrl.ToLower().Contains("content"))
            {
                response = $"{awsSettingCommon.PreviousImageUrl}/{ImageUrl.TrimStart('/')}";
            }

            else if (!useDefaultImage && !string.IsNullOrEmpty(ImageUrl) && ImageUrl.ToLower().Contains(awsSettingCommon.BaseUrl))
                response = ImageUrl;

            else if (!useDefaultImage && !string.IsNullOrEmpty(ImageUrl))
                response = $"{awsSettingCommon.BaseUrl}/{awsSettingCommon.BucketName}/{ImageUrl.TrimStart('/')}";

            return response;
        }
        public static async Task<bool> ImageUpload(AmazonSettingsCommon awsSettingCommon, string fileName, [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return false;

                //string FilePath= $"{ awsSettingCommon.BaseUrl }/{ awsSettingCommon.BucketName}/CUSTOMER/";
                var amazonS3Client = new AmazonS3Client(awsSettingCommon.ConfigAccess, awsSettingCommon.ConfigSecret, RegionEndpoint.APNortheast1);
                var transferUtility = new TransferUtility(amazonS3Client);
                using (var memoryStream = new MemoryStream())
                {
                    // Copy the file to the memory stream
                    await file.CopyToAsync(memoryStream);
                    // Reset the position of the memory stream to the beginning
                    memoryStream.Position = 0;
                    // Upload the file to S3
                    await transferUtility.UploadAsync(memoryStream, awsSettingCommon.BucketName, fileName);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //public static string ProcessedImage(string imageURL, bool useDefaultImage = false)
        //{
        //    string response = $"{_AmazonS3Configruation.BaseURL}/{_AmazonS3Configruation.BucketName}/{_AmazonS3Configruation.NoImageURL.TrimStart('/')}";
        //    if (!useDefaultImage && !string.IsNullOrEmpty(imageURL))
        //        if (imageURL.ToLower().Contains("/content/"))
        //            response = $"{ConfigurationManager.AppSettings["ImageVirtualPath"].ToString()}/{imageURL.TrimStart('/')}";
        //        else if (imageURL.ToLower().Contains(_AmazonS3Configruation.BaseURL))
        //            response = imageURL;
        //        else
        //            response = $"{_AmazonS3Configruation.BaseURL}/{_AmazonS3Configruation.BucketName}/{imageURL.TrimStart('/')}";
        //    return response;
        //}
    }
}
