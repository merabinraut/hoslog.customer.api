using Dapper;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared;
using hoslog.customer.shared.Multitenant;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Multitenant
{
    public class ReservationMultitenantRepository : IReservationMultitenantRepository
    {
        private readonly RepositoryDao _dao;
        private readonly SqlTransactionHandler _sqlTransactionHandler;
        public ReservationMultitenantRepository(IConfiguration configuration, SqlTransactionHandler sqlTransactionHandler)
        {
            _dao = new RepositoryDao(configuration);
            _sqlTransactionHandler = sqlTransactionHandler;
        }

        public async Task<List<ClubReservationTimeResponseMultitenantCommon>> GetAvailableTimeByClub(ClubReservationTimeRequestMultitenantCommon request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@subDomainName", request.subDomainName);
            parameters.Add("@agentId", request.clubId);
            parameters.Add("@date", request.date);
            parameters.Add("@timeZoneOffsetValue", request.timeZoneOffsetValue);
            string procedure = "dbo.apiproc_multitenant_get_club_reservation_available_time_for_given_date";
            return await _dao.ExecuteAsyncList<ClubReservationTimeResponseMultitenantCommon>(procedure, parameters);
        }

        public async Task<CommonDBResponse> IsValidReservation(ConfirmationReservationMultitenantCommon dbRequest)
        {
            var param = new DynamicParameters();
            string sp_name = "[dbo].[apiproc_multitenant_cp_is_valid_reservation]";
            param.Add("@clubId", dbRequest.clubId);
            param.Add("@selectedDateTime", dbRequest.selectedDateTime);
            param.Add("@timeZoneOffsetValue", dbRequest.timeZoneOffsetValue);
            var dbResponse = await _dao.ExecuteAsync<CommonDBResponse>(sp_name, param);
            return dbResponse;
        }

        public async Task<List<PlanMultitenantCommon>> GetPlanList(ConfirmationReservationMultitenantCommon commonModel)
        {
            var param = new DynamicParameters();
            string sp_name = "[dbo].[apiproc_multitenant_cp_reservation_plan_list]";
            param.Add("@clubId", commonModel.clubId);
            param.Add("@selectedDateTime", commonModel.selectedDateTime);
            param.Add("@timeZoneOffsetValue", commonModel.timeZoneOffsetValue);
            param.Add("@noOfPeople", Convert.ToInt32(commonModel.noOfPeople));
            var dbResponse = await _dao.ExecuteAsyncList<PlanMultitenantCommon>(sp_name, param);
            return dbResponse;
        }

        public async Task<List<HostMultitenantCommon>> GetHostList(string? clubId)
        {
            var param = new DynamicParameters();
            string sp_name = "[dbo].[apiproc_multitenant_cp_reservation_host_list]";
            param.Add("@clubId", clubId);
            var dbResponse = await _dao.ExecuteAsyncList<HostMultitenantCommon>(sp_name, param);
            return dbResponse;
        }

        #region Reservation
        public async Task<ConfirmReservationMultitenantResponseCommon> ReservationConfirmation(ConfirmReservationMultitenantRequestCommon request)
        {
            string proc = "dbo.apiproc_multitenant_cp_confirm_reservation";
            var param = new DynamicParameters();
            param.Add("@clubId", request.clubId);
            param.Add("@planId", request.planId);
            param.Add("@noOfPeople", request.noOfPeople);
            param.Add("@hostId", request.hostIdList);
            param.Add("@paymentType", request.paymentType);
            param.Add("@visitDateTime", request.selectedDateTime);
            param.Add("@visitDateTime", request.selectedDateTime);
            param.Add("@actionPlatform", request.actionPlatform);
            param.Add("@actionIP", request.actionIP);
            param.Add("@timeZoneOffsetValue", request.timeZoneOffsetValue);
            param.Add("@referCode", request.referCode);
            param.Add("@referType", request.referType);

            var dbResponse = await _dao.ExecuteAsync<ConfirmReservationMultitenantResponseCommon>(proc, param);
            return dbResponse;
        }
        #endregion

        #region Get reservation details and send otp
        public async Task<GetHoldReservationAndSendOtpResponseCommon> GetHoldReservationAndSendOTP(GetHoldReservationAndSendOtpRequestCommon request)
        {
            string proc = "dbo.apiproc_multitenant_get_hold_reservation_and_send_otp";
            var param = new DynamicParameters();
            param.Add("@mobileNumber", request.mobileNumber);
            param.Add("@reservationHoldId", request.reservationHoldId);
            param.Add("@reservationTraceId", request.reservationTraceId);
            param.Add("@actionPlatform", request.actionPlatform);
            param.Add("@actionIP", request.actionIP);
            
            var dbResponse = await _dao.ExecuteAsync<GetHoldReservationAndSendOtpResponseCommon>(proc, param);
            return dbResponse;
        }

        public async Task<GetHoldReservationAndSendOtpResponseCommon> GetHoldReservationAndResendSendOTP(GetHoldReservationAndSendOtpRequestCommon request)
        {
            string proc = "dbo.apiproc_multitenant_cp_resend_reservation_hold_otp";
            var param = new DynamicParameters();
            param.Add("@mobileNumber", request.mobileNumber);
            param.Add("@reservationHoldId", request.reservationHoldId);
            param.Add("@reservationTraceId", request.reservationTraceId);
            param.Add("@actionIP", request.actionIP);
            param.Add("@actionPlatform", request.actionPlatform);

            var dbResponse = await _dao.ExecuteAsync<GetHoldReservationAndSendOtpResponseCommon>(proc, param);
            return dbResponse;
        }
        #endregion

        #region Reservation hold details and Confiramtion
        public async Task<ReservationHoldDetailMultitenantResponseCommon> GetReservationHoldDetail(string reservationHoldId = "", string reservationTraceId = "")
        {
            string proc = "apiproc_multitenant_cp_get_reservation_hold_detail";
            var param = new DynamicParameters();
            param.Add("@reservationHoldId", reservationHoldId);
            param.Add("@reservationTraceId", reservationTraceId);

            return await _dao.ExecuteAsync<ReservationHoldDetailMultitenantResponseCommon>(proc, param);
        }

        public async Task<ConfirmReservationHoldMultitenantResponseCommon> ConfirmReservationHold(ConfirmReservationHoldMultitenantRequestCommon request)
        {
            string proc = "apiproc_multitenant_cp_confirm_reservation_hold";
            var param = new DynamicParameters();
            param.Add("@reservationHoldId", request.reservationHoldId);
            param.Add("@reservationTraceId", request.reservationTraceId);
            param.Add("@customerId", request.customerId);
            param.Add("@actionIP", request.actionIP);
            param.Add("@actionPlatform", request.actionPlatform);

            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<ConfirmReservationHoldMultitenantResponseCommon>(proc, param);
            return dbResponse;
        }
        #endregion

        #region Registration
        public async Task<CustomerRegistrationMultitenantResponseCommon> RegisterCustomer(CustomerRegistrationMultitenantRequestCommon request)
        {
            string proc = "apiproc_multitenant_cp_customer_registration";
            var param = new DynamicParameters();
            param.Add("@nickName", request.nickName);
            param.Add("@emailAddress", request.emailAddress);
            param.Add("@dob", request.dob);
            param.Add("@mobileNumber", request.mobileNumber);
            param.Add("@actionIP", request.actionIP);
            param.Add("@actionPlatform", request.actionPlatform);
            param.Add("@reservationHoldId", request.reservationHoldId);
            param.Add("@reservationTraceId", request.reservationTraceId);

            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<CustomerRegistrationMultitenantResponseCommon>(proc, param);
            return dbResponse;
        }

        public async Task<CommonDBResponse> UpdateCustomerDetail(CustomerDetailUpdateMultitenantRequestCommon request)
        {
            string proc = "apiproc_multitenant_cp_update_registered_customer_detail";
            var param = new DynamicParameters();
            param.Add("@agentId", request.agentId);
            param.Add("@cognitoUserId", request.cognitoUserId);

            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<CommonDBResponse>(proc, param);
            return dbResponse;
        }
        #endregion

        #region  OTP Verification
        public async Task<OTPVerificationMultitenantResponseCommon> VerifyOTP(OTPVerificationMultitenantRequestCommon request)
        {
            string proc = "dbo.apiproc_multitenant_cp_verify_reservation_hold_otp";
            var param = new DynamicParameters();
            param.Add("@reservationHoldId", request.reservationHoldId);
            param.Add("@reservationTraceId", request.reservationTraceId);
            param.Add("@mobileNumber", request.mobileNumber);
            param.Add("@otpCode", request.otpCode);
            param.Add("@actionIP", request.actionIP);
            param.Add("@actionPlatform", request.actionPlatform);

            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<OTPVerificationMultitenantResponseCommon>(proc, param);
            return dbResponse;
        }
        #endregion
    }
}