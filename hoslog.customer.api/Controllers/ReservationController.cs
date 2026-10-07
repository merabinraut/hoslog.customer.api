using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Library;
using hoslog.customer.business.Conversion;
using hoslog.customer.business.Reservation;
using hoslog.customer.common.Models.ReservationModel;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    [Authorize]
    public class ReservationController : BaseApiController
    {
        private readonly IConversionBusiness _conversion;
        private readonly IReservationBusiness _business;
        public ReservationController(
            IConversionBusiness conversion, IReservationBusiness business)
        {
            _conversion = conversion;
            _business = business;
        }

        #region Reservation Change Request APIs

        #region Club Public Information

        /// <summary>
        /// Gets club schedule list by club ID or club code.
        /// Used for displaying available reservation dates/schedules.
        /// </summary>
        [AllowAnonymous]
        [HttpGet, Route("clubs/schedule")]
        public async Task<IActionResult> GetClubSchedule(
            string clubId = "",
            string clubCode = "",
            int duration = 1)
        {
            var (statusCode, response) = await _business.GetClubSchedule(clubId, clubCode, duration);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        /// Gets available plan list for a club.
        /// If planId is provided, returns specific plan detail.
        /// </summary>
        [AllowAnonymous]
        [HttpGet, Route("clubs/plans")]
        public async Task<IActionResult> GetClubPlanList(
            string clubId = "",
            string clubCode = "",
            string planId = "")
        {
            var (statusCode, response) = await _business.GetClubPlanList(clubId, clubCode, planId);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        /// Gets available time slots for a club on a selected date.
        /// </summary>
        [AllowAnonymous]
        [HttpGet, Route("clubs/timeslots")]
        public async Task<IActionResult> GetClubTimeslots(
            string clubId = "",
            string clubCode = "",
            long date = 0,
            string planId = "")
        {
            var (statusCode, response) = await _business.GetClubTimeslots(clubId, clubCode, date, planId);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        /// Gets host/cast list for a club.
        /// </summary>
        [AllowAnonymous]
        [HttpGet, Route("clubs/hosts")]
        public async Task<IActionResult> GetClubHostList(
            string clubId = "",
            string clubCode = "")
        {
            var (statusCode, response) = await _business.GetClubHostList(clubId, clubCode);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        #endregion


        #region Reservation Setup

        /// <summary>
        /// Gets available reservation preference options.
        /// Example: customer requests, seat preferences, visit preferences, etc.
        /// </summary>
        [AllowAnonymous]
        [HttpGet, Route("reservations/preferences")]
        public async Task<IActionResult> GetReservationPreferences()
        {
            var (statusCode, response) = await _business.GetReservationPreferences();
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        /// Gets available payment methods for reservation payment.
        /// </summary>
        [HttpGet, Route("payments/methods")]
        public async Task<IActionResult> GetReservationPaymentMethods()
        {
            var (statusCode, response) = await _business.GetReservationPaymentMethods();
            return HandleResponseFromStatusCode(statusCode, response);
        }

        #endregion


        #region Reservation Validation and Billing

        /// <summary>
        /// Validates whether the selected reservation date/time and plan are available.
        /// </summary>
        [AllowAnonymous]
        [HttpGet, Route("reservations/validation")]
        public async Task<IActionResult> IsReservationValid(
            string clubId = "",
            string clubCode = "",
            string planId = "",
            long selectedDateTime = 0)
        {
            var (statusCode, response) = await _business.IsReservationValid(
                clubId,
                clubCode,
                planId,
                selectedDateTime
            );

            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        /// Gets reservation billing amount based on club, plan, and number of people.
        /// </summary>
        [HttpGet, Route("reservations/billing")]
        public async Task<IActionResult> GetReservationBilling(
            string clubId = "",
            string clubCode = "",
            string planId = "",
            long visitDateTimeMS = 0,
            int noOfPeople = 0
        )
        {
            var (statusCode, response) = await _business.GetReservationBilling(
                clubId,
                clubCode,
                planId,
                visitDateTimeMS,
                noOfPeople
            );

            return HandleResponseFromStatusCode(statusCode, response);
        }

        #endregion


        #region Reservation Confirmation

        /// <summary>
        /// Confirms the final reservation after validation, billing, OTP/customer flow, and required details.
        /// </summary>
        [HttpPost("reservations/confirm")]
        [LogRequest, LogResponse]
        public async Task<IActionResult> ConfirmReservationAsync(
            [FromBody] ReservationConfirmRequestModel request)
        {
            var session = await _conversion.GetRequiredAsync(HttpContext);
            var (statusCode, response) = await _business.ConfirmReservation(request, session);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        #endregion


        #region Reservation Hold Authentication

        /// <summary>
        /// Creates/confirms a temporary reservation hold and sends OTP for authentication.
        /// </summary>
        [AllowAnonymous]
        [LogRequest, LogResponse]
        [HttpPost("reservations/holds/auth/otp")]
        public async Task<IActionResult> ConfirmReservationHold(
            [FromBody] ReservationHoldConfirmRequestModel request)
        {
            var session = await _conversion.GetRequiredAsync(HttpContext);
            var (statusCode, response) = await _business.ConfirmReservationHold(request, session);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        /// Resends OTP for an existing reservation hold authentication flow.
        /// </summary>
        [AllowAnonymous]
        [LogRequest, LogResponse]
        [HttpPost("reservations/holds/auth/otp/resend")]
        public async Task<IActionResult> ReservationHoldResendOtp(
            [FromBody] ReservationHoldReSendOtpRequestModel request)
        {
            var session = await _conversion.GetRequiredAsync(HttpContext);
            var (statusCode, response) = await _business.ReservationHoldResendOtp(request, session);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        /// Verifies OTP for reservation hold authentication.
        /// </summary>
        [AllowAnonymous]
        [LogRequest, LogResponse]
        [HttpPost("reservations/holds/auth/otp/verify")]
        public async Task<IActionResult> ReservationVerifyOtp(
            [FromBody] ReservationVerifyOtpRequestModel request)
        {
            var session = await _conversion.GetRequiredAsync(HttpContext);
            var (statusCode, response) = await _business.ReservationVerifyOtp(request, session);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        #endregion


        #region Reservation Customer

        /// <summary>
        /// Registers a customer during the reservation flow.
        /// Usually used when the reservation user does not already exist.
        /// </summary>
        [AllowAnonymous]
        [LogRequest, LogResponse]
        [HttpPost("reservations/customers")]
        public async Task<IActionResult> ReservationRegisterCustomer(
            [FromBody] ReservationRegisterCustomerRequestModel request)
        {
            var session = await _conversion.GetRequiredAsync(HttpContext);
            var (statusCode, response) = await _business.ReservationRegisterCustomer(request, session);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        #endregion

        #endregion
    }
}