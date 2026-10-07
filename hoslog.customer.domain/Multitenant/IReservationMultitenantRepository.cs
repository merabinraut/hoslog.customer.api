using hoslog.customer.shared;
using hoslog.customer.shared.Multitenant;

namespace hoslog.customer.domain.Multitenant
{
    public interface IReservationMultitenantRepository
    {
        Task<List<ClubReservationTimeResponseMultitenantCommon>> GetAvailableTimeByClub(ClubReservationTimeRequestMultitenantCommon request);
        Task<CommonDBResponse> IsValidReservation(ConfirmationReservationMultitenantCommon dbRequest);
        Task<List<PlanMultitenantCommon>> GetPlanList(ConfirmationReservationMultitenantCommon commonModel);
        Task<List<HostMultitenantCommon>> GetHostList(string? clubId);
        #region Reservation
        Task<ConfirmReservationMultitenantResponseCommon> ReservationConfirmation(ConfirmReservationMultitenantRequestCommon request);
        #endregion
        #region Get reservation details and send otp
        Task<GetHoldReservationAndSendOtpResponseCommon> GetHoldReservationAndSendOTP(GetHoldReservationAndSendOtpRequestCommon request);
        Task<GetHoldReservationAndSendOtpResponseCommon> GetHoldReservationAndResendSendOTP(GetHoldReservationAndSendOtpRequestCommon request);
        #endregion
        #region Reservation hold details and Confiramtion
        Task<ReservationHoldDetailMultitenantResponseCommon> GetReservationHoldDetail(string reservationHoldId = "", string reservationTraceId = "");
        Task<ConfirmReservationHoldMultitenantResponseCommon> ConfirmReservationHold(ConfirmReservationHoldMultitenantRequestCommon request);
        #endregion

        #region registration
        Task<CustomerRegistrationMultitenantResponseCommon> RegisterCustomer(CustomerRegistrationMultitenantRequestCommon request);
        Task<CommonDBResponse> UpdateCustomerDetail(CustomerDetailUpdateMultitenantRequestCommon request);
        #endregion
        #region  OTP Verification
        Task<OTPVerificationMultitenantResponseCommon> VerifyOTP(OTPVerificationMultitenantRequestCommon request);
        #endregion
    }
}