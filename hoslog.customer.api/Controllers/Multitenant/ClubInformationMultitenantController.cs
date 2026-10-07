// using Asp.Versioning;
// using hoslog.customer.api.Filters;
// using hoslog.customer.business.Multitenant;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace hoslog.customer.api.Controllers.Multitenant
// {
//     [Route("api/v{version:apiVersion}")]
//     [ApiVersion("1.0")]
//     [ServiceFilter(typeof(SubDomainNameValidatorFilter))]
//     public class ClubInformationMultitenantController : BaseApiController
//     {
//         private readonly IClubInformationMultitenantBusiness _clubInformationBusiness;
//         public ClubInformationMultitenantController(IClubInformationMultitenantBusiness clubInformationBusiness)
//         {
//             _clubInformationBusiness = clubInformationBusiness;
//         }

//         [AllowAnonymous]
//         [HttpGet("club-basic-information-multitenant")]
//         public async Task<IActionResult> GetClubBasicInformation()
//         {
//             var (statusCode, response) = await _clubInformationBusiness.GetClubBasicInfomration(string.Empty, string.Empty);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [AllowAnonymous]
//         [HttpGet("club-review-and-rating-multitenant")]
//         public async Task<IActionResult> GetClubReviewAndRatings(string? clubId = null, string? clubCode = null, int pageNo = 1)
//         {
//             var (statusCode, response) = await _clubInformationBusiness.GetClubReviewAndRatings(clubId, clubCode, pageNo);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [AllowAnonymous]
//         [HttpGet, Route("club-weekly-schedule-multitenant")]
//         public async Task<IActionResult> GetClubWeeklySchedule(int duration, string? clubId = null, string? clubCode = null)
//         {
//             var (statusCode, response) = await _clubInformationBusiness.GetClubWeeklySchedule(clubId, clubCode, duration);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [AllowAnonymous]
//         [HttpGet, Route("club-schedule-multitenant")]
//         public async Task<IActionResult> GetScheduleList(long? date, string? clubId = null, string? clubCode = null)
//         {
//             var (statusCode, response) = await _clubInformationBusiness.GetScheduleListAsync(clubId, clubCode, date);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [HttpGet, Route("club-notice-multitenant")]
//         public async Task<IActionResult> GetclubNotice(int pageNo, string clubId = "", string clubCode = "")
//         {
//             var (statusCode, response) = await _clubInformationBusiness.GetClubNoticeAsync(pageNo, clubId, clubCode);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [HttpGet, Route("gallery-image-multitenant")]
//         public async Task<IActionResult> GetGalleryImages(string clubId = "", string clubCode = "")
//         {
//             var (statusCode, response) = await _clubInformationBusiness.GetGalleryPhotoAsync(clubId, clubCode);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }
//     }
// }