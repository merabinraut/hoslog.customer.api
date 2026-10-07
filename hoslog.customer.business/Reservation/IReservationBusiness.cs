using System.Net;
using hoslog.customer.common.Models.Conversion;
using hoslog.customer.common.Models.ReservationModel;
using hoslog.customer.shared;

namespace hoslog.customer.business.Reservation;

public interface IReservationBusiness
{
    Task<(HttpStatusCode, object)> GetAvailableTimeByClub(string clubId, long? date);
    Task<(HttpStatusCode, object)> GetBillingDetails(string? clubId, int? noOfPeople, string? planId);
    Task<(HttpStatusCode, object)> IsValidReservation(string clubId, string userId, string noOfPeople, long selectedDateTime, AmazonSettingsCommon mappedAwsSetting);
    Task<(HttpStatusCode, object)> ReservationCheckout(ReservationCheckoutRequestModel dbRequest);

    #region Reservation Change Request
    Task<(HttpStatusCode, CommonApiResponse)> GetClubSchedule(string clubId, string clubCode = "", int duration = 1);

    Task<(HttpStatusCode, CommonApiResponse)> GetClubPlanList(string clubId, string clubCode = "", string planId = "");

    Task<(HttpStatusCode, CommonApiResponse)> GetClubTimeslots(string clubId, string clubCode = "", long date = 0, string planId = "");
    Task<(HttpStatusCode, CommonApiResponse)> GetClubHostList(string clubId = "", string clubCode = "");
    Task<(HttpStatusCode, CommonApiResponse)> GetReservationPreferences();
    Task<(HttpStatusCode, CommonApiResponse)> GetReservationPaymentMethods();
    Task<(HttpStatusCode, CommonApiResponse)> IsReservationValid(
        string clubId = "",
        string clubCode = "",
        string planId = "",
        long selectedDateTime = 0
    );
    Task<(HttpStatusCode, CommonApiResponse)> GetReservationBilling(
       string clubId = "",
       string clubCode = "",
       string planId = "",
       long visitDateTimeMS = 0,
       int noOfPeople = 0
    );
    Task<(HttpStatusCode, object)> ConfirmReservation(ReservationConfirmRequestModel request,ConversionSessionContext session);
    #region Auth
    Task<(HttpStatusCode, object)> ConfirmReservationHold(ReservationHoldConfirmRequestModel request,ConversionSessionContext session);
    Task<(HttpStatusCode, object)> ReservationVerifyOtp(ReservationVerifyOtpRequestModel request,ConversionSessionContext session);
    Task<(HttpStatusCode, object)> ReservationHoldResendOtp(ReservationHoldReSendOtpRequestModel request,ConversionSessionContext session);
    Task<(HttpStatusCode, object)> ReservationRegisterCustomer(ReservationRegisterCustomerRequestModel request,ConversionSessionContext session);
    #endregion
    #endregion
}
