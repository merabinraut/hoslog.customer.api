using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using System.Net;
using hoslog.customer.shared;
using Newtonsoft.Json;
using hoslog.customer.common.Service;



namespace hoslog.customer.api.Library
{
    public class ImageHelper
    {
        private readonly AppSettingData _awsSettings = new AppSettingData();
        private readonly ApiResponseMessageService _apiResponseMessageService;
        public ImageHelper(ApiResponseMessageService apiResponseMessageService)
        {
            _apiResponseMessageService = apiResponseMessageService;
        }
        public async Task<(HttpStatusCode, object)> GetPresignedUrlAsync(string KeyName)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            if (string.IsNullOrEmpty(KeyName))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = ResponseMessages.Msg400_BadRequest;
                resp.data = Array.Empty<string>();
                resp.errors = new List<Errors>
                 {
                   new Errors { errorCode =  ResponseCodes.CodeCUEPF102_Failed_to_generate_URL,errorMessage =await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUEPF102_Failed_to_generate_URL) }
                 };
                return await Task.FromResult((HttpStatusCode.BadRequest, resp));
            }

            try
            {
                // AWSConfigsS3.UseSignatureVersion4 = true;
                IAmazonS3 client = new AmazonS3Client(_awsSettings.ConfigAccess, _awsSettings.ConfigSecret, RegionEndpoint.APNortheast1);

                string url = GeneratePreSignedURL(client, _awsSettings.BucketName, KeyName, _awsSettings.TimeOutDuration);

                if (!string.IsNullOrEmpty(url))
                {
                    resp.code = ResponseCodes.Code200_Success;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUSPF105_File_uploaded_successfully) ;
                    resp.data = url;
                    return await Task.FromResult((HttpStatusCode.OK, resp));
                }
                else
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = ResponseMessages.Msg400_BadRequest;
                    resp.data = Array.Empty<string>();
                    resp.errors = new List<Errors>
                 {
                   new Errors { errorCode =  ResponseCodes.CodeCUEPF102_Failed_to_generate_URL,errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUEPF102_Failed_to_generate_URL)}
                 };
                    return await Task.FromResult((HttpStatusCode.InternalServerError, resp));
                }
            }
            catch (Exception ex)
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = ResponseMessages.Msg400_BadRequest;
                resp.data = Array.Empty<string>();
                resp.errors = new List<Errors>
                 {
                   new Errors { errorCode =  resp.code ,errorMessage = $"Exception occurred: {ex.Message}"}
                 };
                return await Task.FromResult((HttpStatusCode.BadRequest, resp));
            }
        }

        private string GeneratePreSignedURL(IAmazonS3 client, string bucketName, string objectKey, double duration)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(duration),
            };

            string url = client.GetPreSignedURL(request);
            return url;
        }
        public string GetProcessedImageUrl(string imageUrl, bool useDefaultImage = false)
        {
            string processedUrl = $"{_awsSettings.BaseUrl}/{_awsSettings.BucketName}/{_awsSettings.PreviousImageUrl.TrimStart('/')}";
            if (!useDefaultImage && !string.IsNullOrEmpty(imageUrl))
            {
                if (imageUrl.ToLower().Contains("/content/userupload"))
                {
                    processedUrl = $"{_awsSettings.PreviousImageUrl}/{imageUrl.TrimStart('/')}";
                }
                else
                {
                    processedUrl = $"{_awsSettings.BaseUrl}/{_awsSettings.BucketName}/{imageUrl.TrimStart('/')}";
                }
            }
            return processedUrl;
        }
    }
}
