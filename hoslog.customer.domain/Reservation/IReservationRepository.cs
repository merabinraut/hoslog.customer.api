using hoslog.customer.common.Models.ReservationModel;
using hoslog.customer.shared;
using hoslog.customer.shared.Multitenant;
using hoslog.customer.shared.Reservation;
using hoslog.customer.shared.ReservationModel;

namespace hoslog.customer.domain.Reservation;

public interface IReservationRepository
{
    Task<List<ClubReservationTimeResponseCommon>> GetAvailableTimeByClub(ClubReservationTimeRequestCommon request);
    Task<IList<BillingDetailsCommonModel>> GetBillingDetails(BilligDetailRequestModel dbRequest);
    Task<IList<ClubBasicDetailModel>> GetClubDetail(string? clubId);
    Task<IList<HostModel>> GetHostList(string? clubId);
    Task<IList<PaymentMethodModel>> GetPaymentMethodList(string agentId = "");
    Task<IList<PlanModel>> GetPlanList(ConfirmationReservationCommon commonModel);
    Task<CommonDBResponse> IsValidReservation(ConfirmationReservationCommon dbRequest);
    Task<ReservationCheckoutResponseCommon> ReservationCheckout(ReservationCheckoutRequestCommon commonDbRequest);
    Task<string> GetMessageToSendInLine(LineGroupAfterReservationCommon commonDbRequest);
    Task<List<ClubSpecificPlanResponseCommon>> GetClubSpecificPlanList(string clubId);

    #region Reservation Change Request
    Task<List<ReservationScheduleResponseCommon>> GetClubSchedule(ReservationScheduleRequestCommon request);

    Task<List<ReservationPlanListResponseCommon>> GetClubPlanList(ReservationPlanListRequestCommon request);

    Task<List<ReservationTimeslotsResponseCommon>> GetClubTimeslots(ReservationTimeslotsRequestCommon request);
    Task<List<ReservationHostListResponseCommon>> GetClubHostList(long clubId = 0, string clubCode = "");
    Task<string> GetReservationPreferences();
    Task<CommonDBResponse> IsReservationValid(ReservationValidateRequestCommon request);

    Task<ReservationBillingResponseCommon> GetReservationBilling(ReservationBillingRequestCommon request);

    Task<List<ReservationPaymentMethodsResponseCommon>> GetReservationPaymentMethods(string userId = "");
    Task<ReservationConfirmResponseCommon> ConfirmReservationAsync(ReservationConfirmRequestCommon request);
    #region OTP Section
    Task<ReservationSendOtpResponseCommon> ReservationSendOtp(ReservationSendOtpRequestCommon request);
    Task<ReservationReSendOtpResponseCommon> ReservationReSendOtp(ReservationReSendOtpRequestCommon request);
    Task<ReservationVerifyOtpResponseCommon> ReservationVerifyOtp(ReservationVerifyOtpRequestCommon request);
    Task<CommonDBResponse> ReservationUpdateCognitoSession(ReservationUpdateCognitoSessionRequestCommon request);
    Task<ReservationHoldConfirmResponseCommon> ConfirmReservationHold(ReservationHoldConfirmRequestCommon request);
    Task<ReservationRegisterCustomerResponseCommon> ReservationRegisterCustomer(ReservationRegisterCustomerRequestCommon request);
    Task<ReservationHoldDetailResponseCommon> ReservationHoldDetails(long reservationHoldId);
    Task<ReservationHoldDetailResponseCommon> ReservationHoldConfirmedDetails(long reservationHoldId);
    Task<CommonDBResponse> ReservationUpdateCustomerDetail(ReservationCustomerDetailsUpdateRequestCommon request);
    #endregion
    #endregion
}
