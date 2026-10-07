using Dapper;
using hoslog.customer.common.Models.ReservationModel;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared;
using hoslog.customer.shared.Multitenant;
using hoslog.customer.shared.Reservation;
using hoslog.customer.shared.ReservationModel;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Reservation;

public class ReservationRepository : IReservationRepository
{
    private readonly RepositoryDao _dao;
    private readonly SqlTransactionHandler _sqlTransactionHandler;
    public ReservationRepository
    (
        IConfiguration configuration,
        SqlTransactionHandler sqlTransactionHandler
    )
    {
        _dao = new RepositoryDao(configuration);
        _sqlTransactionHandler = sqlTransactionHandler;
    }

    public async Task<List<ClubReservationTimeResponseCommon>> GetAvailableTimeByClub(ClubReservationTimeRequestCommon request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@agentId", request.clubId);
        parameters.Add("@date", request.date);
        parameters.Add("@timeZoneOffsetValue", request.timeZoneOffsetValue);
        string procedure = "dbo.apiproc_get_club_reservation_available_time_for_given_date";
        return await _dao.ExecuteAsyncList<ClubReservationTimeResponseCommon>(procedure, parameters);
    }

    public async Task<IList<BillingDetailsCommonModel>> GetBillingDetails(BilligDetailRequestModel dbRequest)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_reservation_billing_details]";
        param.Add("@userId", dbRequest.userId);
        param.Add("@planId", dbRequest.planId);
        param.Add("@clubId", dbRequest.clubId);
        param.Add("@noOfPeople", dbRequest.noOfPeople);
        var dbResponse = await _dao.ExecuteAsyncList<BillingDetailsCommonModel>(sp_name, param);
        return dbResponse;
    }

    public async Task<IList<ClubBasicDetailModel>> GetClubDetail(string? clubId)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_get_club_detail]";
        param.Add("@clubId", clubId);
        var dbResponse = await _dao.ExecuteAsyncList<ClubBasicDetailModel>(sp_name, param);
        return dbResponse;
    }

    public async Task<IList<HostModel>> GetHostList(string? clubId)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_reservation_host_list]";
        param.Add("@clubId", clubId);
        var dbResponse = await _dao.ExecuteAsyncList<HostModel>(sp_name, param);
        return dbResponse;
    }

    public async Task<IList<PaymentMethodModel>> GetPaymentMethodList(string agentId = "")
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_payment_method_list]";
        param.Add("@customerId", agentId);
        var dbResponse = await _dao.ExecuteAsyncList<PaymentMethodModel>(sp_name, param);
        return dbResponse;
    }

    public async Task<IList<PlanModel>> GetPlanList(ConfirmationReservationCommon commonModel)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_reservation_plan_list]";
        param.Add("@userId", commonModel.userId);
        param.Add("@clubId", commonModel.clubId);
        param.Add("@selectedDateTime", commonModel.selectedDateTime);
        param.Add("@timeZoneOffsetValue", commonModel.timeZoneOffsetValue);
        param.Add("@noOfPeople", Convert.ToInt32(commonModel.noOfPeople));
        var dbResponse = await _dao.ExecuteAsyncList<PlanModel>(sp_name, param);
        return dbResponse;
    }

    public async Task<CommonDBResponse> IsValidReservation(ConfirmationReservationCommon dbRequest)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_is_valid_reservation]";
        param.Add("@userId", dbRequest.userId);
        param.Add("@clubId", dbRequest.clubId);
        param.Add("@selectedDateTime", dbRequest.selectedDateTime);
        param.Add("@timeZoneOffsetValue", dbRequest.timeZoneOffsetValue);
        var dbResponse = await _dao.ExecuteAsync<CommonDBResponse>(sp_name, param);
        return dbResponse;
    }

    public async Task<ReservationCheckoutResponseCommon> ReservationCheckout(ReservationCheckoutRequestCommon commonDbRequest)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_reservation_transaction]";
        param.Add("@userId", commonDbRequest.userId);
        param.Add("@clubId", commonDbRequest.clubId);
        param.Add("@visitDateTime", commonDbRequest.selectedDateTime);
        param.Add("noOfPeople", commonDbRequest.noOfPeople);
        param.Add("@hostIdList", commonDbRequest.hostIdList);
        param.Add("@planId", commonDbRequest.planId);
        param.Add("@paymentType", commonDbRequest.paymentType);
        param.Add("@actionUser", commonDbRequest.actionUser);
        param.Add("@actionPlatform", commonDbRequest.actionPlatform);
        param.Add("@actionIP", commonDbRequest.actionIP);
        param.Add("@timeZoneOffsetValue", commonDbRequest.timeZoneOffsetValue);
        var dbResponse = await _dao.ExecuteAsync<ReservationCheckoutResponseCommon>(sp_name, param);
        return dbResponse;
    }
    public async Task<string> GetMessageToSendInLine(LineGroupAfterReservationCommon commonDbRequest)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_line_group_message_after_reservation]";
        param.Add("@flag", commonDbRequest.flag);
        param.Add("@clubId", commonDbRequest.clubId);
        param.Add("@visitDateTime", commonDbRequest.selectedDateTime);
        param.Add("noOfPeople", commonDbRequest.noOfPeople);
        param.Add("@planId", commonDbRequest.planId);
        param.Add("@username", commonDbRequest.userName);
        param.Add("@reservationId", commonDbRequest.reservationId);
        param.Add("@planAmount", commonDbRequest.planAmount);
        param.Add("@planTime", commonDbRequest.planTime);
        param.Add("@timeZoneOffsetValue", commonDbRequest.timeZoneOffsetValue);
        param.Add("@otpCode", commonDbRequest.otpCode);
        var dbResponse = await _dao.ExecuteAsync<string>(sp_name, param);
        return dbResponse;
    }

    public async Task<List<ClubSpecificPlanResponseCommon>> GetClubSpecificPlanList(string clubId)
    {
        string proc = "apiproc_cp_get_club_own_plan";
        var parameters = new DynamicParameters();
        parameters.Add("@clubId", clubId);
        var dbResp = await _dao.ExecuteAsyncList<ClubSpecificPlanResponseCommon>(proc, parameters);
        return dbResp;
    }

    #region Reservation Change Request
    public async Task<List<ReservationScheduleResponseCommon>> GetClubSchedule(ReservationScheduleRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_get_club_schedule";
        var parameters = new DynamicParameters(new
        {
            request.clubId,
            request.clubCode,
            request.duration,
            request.timeZoneOffsetValue
        });
        return await _dao.ExecuteAsyncList<ReservationScheduleResponseCommon>(proc, parameters);
    }

    public async Task<List<ReservationPlanListResponseCommon>> GetClubPlanList(ReservationPlanListRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_get_plan_list";
        var parameters = new DynamicParameters(new
        {
            request.clubId,
            request.clubCode,
            request.timeZoneOffsetValue,
            request.planId
        });
        parameters.Add("@userId", request.actionUserId);
        return await _dao.ExecuteAsyncList<ReservationPlanListResponseCommon>(proc, parameters);
    }

    public async Task<List<ReservationTimeslotsResponseCommon>> GetClubTimeslots(ReservationTimeslotsRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_get_club_available_timeslots";
        var parameters = new DynamicParameters(new
        {
            request.clubId,
            request.clubCode,
            request.date,
            request.planId,
            request.timeZoneOffsetValue
        });
        return await _dao.ExecuteAsyncList<ReservationTimeslotsResponseCommon>(proc, parameters);
    }

    public async Task<List<ReservationHostListResponseCommon>> GetClubHostList(long clubId = 0, string clubCode = "")
    {
        string proc = "dbo.apiproc_cp_reservation_get_host";
        var parameters = new DynamicParameters(new
        {
            clubId,
            clubCode
        });
        return await _dao.ExecuteAsyncList<ReservationHostListResponseCommon>(proc, parameters);
    }

    public async Task<string> GetReservationPreferences()
    {
        string proc = "dbo.apiproc_cp_reservation_get_preferences";
        return await _dao.ExecuteAsync<string>(proc, new DynamicParameters());
    }

    public async Task<CommonDBResponse> IsReservationValid(ReservationValidateRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_validate";
        var parameters = new DynamicParameters(new
        {
            request.clubCode,
            request.clubId,
            request.planId,
            request.selectedDateTime,
            request.timeZoneOffsetValue
        });
        parameters.Add("@userId", request.actionUserId);
        return await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
    }

    public async Task<ReservationBillingResponseCommon> GetReservationBilling(ReservationBillingRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_get_billing";
        var parameters = new DynamicParameters(new
        {
            request.clubCode,
            request.clubId,
            request.planId,
            request.noOfPeople,
            request.timeZoneOffsetValue
        });
        parameters.Add("@userId", request.actionUserId);
        return await _dao.ExecuteAsync<ReservationBillingResponseCommon>(proc, parameters);
    }

    public async Task<List<ReservationPaymentMethodsResponseCommon>> GetReservationPaymentMethods(string userId = "")
    {
        string proc = "dbo.apiproc_cp_reservation_get_payment_methods";
        var parameters = new DynamicParameters();
        parameters.Add("@userId", userId);
        return await _dao.ExecuteAsyncList<ReservationPaymentMethodsResponseCommon>(proc, parameters);
    }

    public async Task<ReservationConfirmResponseCommon> ConfirmReservationAsync(ReservationConfirmRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_transaction_v2";
        var parameters = new DynamicParameters(new
        {
            request.clubId,
            request.clubCode,
            request.noOfPeople,
            request.hostIds,
            request.planId,
            request.paymentType,
            request.visitDateTimeMS,
            request.timeZoneOffsetValue,
            request.actionIP,
            request.actionPlatform,
            request.customerPreferencesJson,
            request.reservationHoldId
        });
        parameters.Add("@userId", request.actionUserId);
        parameters.Add("@actionUser", request.actionUserId);
        return await _dao.ExecuteAsync<ReservationConfirmResponseCommon>(proc, parameters);
    }

    #region OTP Section
    public async Task<ReservationSendOtpResponseCommon> ReservationSendOtp(ReservationSendOtpRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_send_otp";
        var parameters = new DynamicParameters(new
        {
            request.mobileNumber,
            request.reservationHoldId,
            request.otpCode,
            request.otpHash,
            request.actionIP,
            request.actionPlatform,
            request.traceId,
            request.cognitoAuthSession,
            request.username
        });
        return await _sqlTransactionHandler.ExecuteWithTransactionAsync<ReservationSendOtpResponseCommon>(proc, parameters);
    }

    public async Task<ReservationReSendOtpResponseCommon> ReservationReSendOtp(ReservationReSendOtpRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_hold_confirm_resend_otp";
        var parameters = new DynamicParameters(new
        {
            request.mobileNumber,
            request.reservationHoldId,
            request.otpCode,
            request.otpHash,
            request.actionIP,
            request.actionPlatform,
            request.cognitoAuthSession
        });
        return await _dao.ExecuteAsync<ReservationReSendOtpResponseCommon>(proc, parameters);
    }

    public async Task<ReservationVerifyOtpResponseCommon> ReservationVerifyOtp(ReservationVerifyOtpRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_verify_otp";
        var parameters = new DynamicParameters(new
        {
            request.mobileNumber,
            request.reservationHoldId,
            request.cognitoPurpose,
            request.submittedOtpHash,
            request.actionIP,
            request.actionPlatform,
            request.isViaMultitenant
        });
        return await _dao.ExecuteAsync<ReservationVerifyOtpResponseCommon>(proc, parameters);
    }

    public async Task<CommonDBResponse> ReservationUpdateCognitoSession(ReservationUpdateCognitoSessionRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_update_cognito_session";
        var parameters = new DynamicParameters(new
        {
            request.reservationHoldId,
            request.processId,
            request.cognitoAuthSession
        });
        return await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
    }

    public async Task<ReservationHoldConfirmResponseCommon> ConfirmReservationHold(ReservationHoldConfirmRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_hold_confirm";
        var parameters = new DynamicParameters(new
        {
            request.clubId,
            request.noOfPeople,
            request.hostIds,
            request.planId,
            request.visitDateTimeMS,
            request.timeZoneOffsetValue,
            request.actionIP,
            request.actionPlatform,
            request.customerPreferencesJson,
            request.isViaMultitenant,
            request.mobileNumber,
            request.referCode,
            request.referType
        });
        parameters.Add("@actionUser", request.actionUserId);
        return await _sqlTransactionHandler.ExecuteWithTransactionAsync<ReservationHoldConfirmResponseCommon>(proc, parameters);
    }

    public async Task<ReservationRegisterCustomerResponseCommon> ReservationRegisterCustomer(ReservationRegisterCustomerRequestCommon request)
    {
        string proc = "dbo.apiproc_cp_reservation_register_customer";
        var parameters = new DynamicParameters(new
        {
            request.nickName,
            request.emailAddress,
            request.mobileNumber,
            request.dob,
            request.reservationHoldId,
            request.actionPlatform,
            request.actionIP,
            request.isViaMultitenant
        });
        return await _sqlTransactionHandler.ExecuteWithTransactionAsync<ReservationRegisterCustomerResponseCommon>(proc, parameters);
    }

    public async Task<ReservationHoldDetailResponseCommon> ReservationHoldDetails(long reservationHoldId)
    {
        string proc = "dbo.apiproc_cp_reservation_get_hold_details";
        var parameters = new DynamicParameters(new
        {
            reservationHoldId
        });
        return await _dao.ExecuteAsync<ReservationHoldDetailResponseCommon>(proc, parameters);
    }

    public async Task<ReservationHoldDetailResponseCommon> ReservationHoldConfirmedDetails(long reservationHoldId)
    {
        string proc = "dbo.apiproc_cp_reservation_get_hold_confirmed_details";
        var parameters = new DynamicParameters(new
        {
            reservationHoldId
        });
        return await _dao.ExecuteAsync<ReservationHoldDetailResponseCommon>(proc, parameters);
    }

    public async Task<CommonDBResponse> ReservationUpdateCustomerDetail(ReservationCustomerDetailsUpdateRequestCommon request)
    {
        string proc = "apiproc_cp_reservation_update_registered_customer";
        var param = new DynamicParameters();
        param.Add("@agentId", request.agentId);
        param.Add("@cognitoUserId", request.cognitoUserId);

        var dbResponse = await _dao.ExecuteAsync<CommonDBResponse>(proc, param);
        return dbResponse;
    }
    #endregion
    #endregion
}
