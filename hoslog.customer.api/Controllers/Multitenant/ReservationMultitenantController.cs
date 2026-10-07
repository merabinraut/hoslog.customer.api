// using Asp.Versioning;
// using hoslog.customer.api.Filters;
// using hoslog.customer.business.Multitenant;
// using hoslog.customer.common.Models.Multitenant;
// using Microsoft.AspNetCore.Mvc;

// namespace hoslog.customer.api.Controllers.Multitenant
// {
//     [Route("api/v{version:apiVersion}")]
//     [ApiVersion("1.0")]
//     [ServiceFilter(typeof(SubDomainNameValidatorFilter))]
//     public class ReservationMultitenantController : BaseApiController
//     {
//         private readonly IReservationMultitenantBusiness _reservationMultitenantBusiness;
//         public ReservationMultitenantController(IReservationMultitenantBusiness reservationMultitenantBusiness)
//         {
//             _reservationMultitenantBusiness = reservationMultitenantBusiness;
//         }

//         // [LogRequest, LogResponse]
//         [HttpGet, Route("available-time-multitenant")]
//         public async Task<IActionResult> GetAvailableTimeByClub(long? date, string clubId = "")
//         {
//             var (statusCode, response) = await _reservationMultitenantBusiness.GetAvailableTimeByClub(date, clubId);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [HttpGet, Route("is-valid-reservation-multitenant")]
//         public async Task<IActionResult> IsValidReservation(long selectedDateTime, string clubId, string noOfPeople)
//         {
//             var (statusCode, response) = await _reservationMultitenantBusiness.IsValidReservation(clubId, noOfPeople, selectedDateTime);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [LogRequest, LogResponse]
//         [HttpPost, Route("reservation-checkout-multitenant")]
//         public async Task<IActionResult> ReservationCheckout([FromBody] ConfirmReservationMultitenantRequestModel dbRequest)
//         {
//             var (statusCode, response) = await _reservationMultitenantBusiness.ReservationConfirmation(dbRequest);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [LogRequest, LogResponse]
//         [HttpPost, Route("reservation-verification-multitenant")]
//         public async Task<IActionResult> GetHoldReservationAndSendOTP(GetHoldReservationAndSendOtpRequestModel request)
//         {
//             var (statusCode, response) = await _reservationMultitenantBusiness.GetHoldReservationAndSendOTP(request);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [LogRequest, LogResponse]
//         [HttpPost, Route("reservation-verification-and-resend-otp-multitenant")]
//         public async Task<IActionResult> GetHoldReservationAndReSendOTP(GetHoldReservationAndSendOtpRequestModel request)
//         {
//             var (statusCode, response) = await _reservationMultitenantBusiness.GetHoldReservationAndReSendResendOTP(request);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [LogRequest, LogResponse]
//         [HttpPost, Route("confirm-existing-customer-reservation-hold-multitenant")]
//         public async Task<IActionResult> ConfirmReservationHold(ConfirmReservationHoldMultitenantRequestModel request)
//         {
//             var (statusCode, response) = await _reservationMultitenantBusiness.ConfirmReservationHold(request);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [LogRequest, LogResponse]
//         [HttpPost, Route("validate-non-existing-customer-reservation-hold-otp-multitenant")]
//         public async Task<IActionResult> VerifyOTP(OTPVerificationMultitenantRequestModel request)
//         {
//             var (statusCode, response) = await _reservationMultitenantBusiness.VerifyOTP(request);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }

//         [LogRequest, LogResponse]
//         [HttpPost, Route("confirm-non-existing-customer-reservation-hold-multitenant")]
//         public async Task<IActionResult> ConfirmNonExistingCustomerReservationHold(NonExistingCustomerReservationConfirmMultitenantRequestCommon request)
//         {
//             var (statusCode, response) = await _reservationMultitenantBusiness.ConfirmNonExistingCustomerReservationHold(request);
//             return HandleResponseFromStatusCode(statusCode, response);
//         }
//     }
// }