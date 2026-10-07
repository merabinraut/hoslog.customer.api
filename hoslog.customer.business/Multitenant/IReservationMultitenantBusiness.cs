using System.Net;
using hoslog.customer.common.Models.Multitenant;
using hoslog.customer.shared;

namespace hoslog.customer.business.Multitenant
{
    public interface IReservationMultitenantBusiness
    {
        Task<(HttpStatusCode, CommonApiResponse)> GetAvailableTimeByClub(long? date, string clubId = "");
        Task<(HttpStatusCode, object)> IsValidReservation(string clubId, string noOfPeople, long selectedDateTime);
        #region Reservation
        Task<(HttpStatusCode, object)> ReservationConfirmation(ConfirmReservationMultitenantRequestModel request);
        #endregion
        #region Get reservation details and send otp
        Task<(HttpStatusCode, object)> GetHoldReservationAndSendOTP(GetHoldReservationAndSendOtpRequestModel request);
        Task<(HttpStatusCode, object)> GetHoldReservationAndReSendResendOTP(GetHoldReservationAndSendOtpRequestModel request);
        #endregion
        #region Confirm reservation hold
        Task<(HttpStatusCode, object)> ConfirmReservationHold(ConfirmReservationHoldMultitenantRequestModel request);
        #endregion
        Task<(HttpStatusCode, object)> VerifyOTP(OTPVerificationMultitenantRequestModel request);
        #region  Reservation
        Task<(HttpStatusCode, object)> ConfirmNonExistingCustomerReservationHold(NonExistingCustomerReservationConfirmMultitenantRequestCommon request);
        #endregion
    }
}