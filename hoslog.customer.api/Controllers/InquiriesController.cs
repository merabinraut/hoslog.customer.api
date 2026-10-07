using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Library;
using hoslog.customer.business.Inquiries;
using hoslog.customer.common.Models.EnqueryModel;
using hoslog.customer.shared;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class InquiriesController : BaseApiController
    {
        private readonly IInquiriesBusiness _inquiriesBusiness;
        private readonly ImageHelper _imageHelper;

        public InquiriesController(IInquiriesBusiness inquiriesBusiness, ImageHelper imageHelper)
        {
            _inquiriesBusiness = inquiriesBusiness;
            _imageHelper = imageHelper;
        }
        /// <summary>
        /// post inquiry
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request form data :**
        ///
        ///  {
        ///  
        ///        {
        ///         "name": "full name for inquery",
        ///         "emailAddress": "abc@email.com",
        ///         "mobileNumber": "09155454545",
        ///         "enqueryType": "_pSrGhvssrrewrwerw", Encrypted enquery type which is provided to the common attributes
        ///         "subject": "科学",
        ///         "description": "それはとてもとても",
        ///         "attachment": "prodoa.jpg",
        ///         "companyName": "",
        ///         "actionPlatform":"web"
        ///       }
        ///        
        ///  }
        ///  
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>
        [LogRequest]
        [LogResponse]
        [HttpPost, Route("post-inquiry")]
        public async Task<IActionResult> PostInquiries([FromForm] InquiriesRequestModel request)
        {
            string path = "";
            if (request.attachment != null)
            {
                long miliSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string firstName = request.name.Split(' ')[0];
                string generatedImageName = firstName + "" + miliSeconds;
                string fileExtension = Path.GetExtension(request.attachment.FileName).ToLower();
                var (responseCodeImage, responseImage) = await _imageHelper.GetPresignedUrlAsync("INQUIRY/" + generatedImageName + fileExtension);
                var responseImageObject = responseImage.Adapt<CommonApiResponse>();
                string responseJson = responseImageObject.data.ToString();
                var client = new HttpClient();
                using (var memoryStream = new MemoryStream())
                {
                    await request.attachment.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();

                    var imageContent = new ByteArrayContent(imageBytes);
                    string mimeType = fileExtension.ToLower() == ".png" ? "image/png" : (fileExtension.ToLower() == ".jpeg" || fileExtension.ToLower() == ".jpg" ? "image/jpeg" : (fileExtension.ToLower() == ".pdf" ? "application/pdf" : "application/octet-stream"));
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
                    HttpResponseMessage imageResponse = await client.PutAsync(responseJson, imageContent);                    
                }
                string url_parts = responseJson.Split('?')[0];
                int startIndex = url_parts.IndexOf("/ADMIN");
                path = (url_parts.Contains("/INQUIRY")) ? url_parts : "/INQUIRY/" + url_parts;
            }
            var (statusCode, response) = await _inquiriesBusiness.PostInquiries(request, path);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
