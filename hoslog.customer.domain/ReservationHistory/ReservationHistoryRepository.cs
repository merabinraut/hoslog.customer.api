using Dapper;
using hoslog.customer.common.Models;
using hoslog.customer.common.Models.Reservation;
using hoslog.customer.shared;
using hoslog.customer.shared.Reservation;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Reservation
{
    public class ReservationHistoryRepository : IReservationHistoryRepository
    {
        private readonly RepositoryDao _dao;

        public ReservationHistoryRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);

        }
        public async Task<List<ReservationCommon>> GetReservationList(PaginationRequestModel objPaginationRequestModel, string userId, string reservationStatus = "")
        {
            string proc = "apiproc_cp_reservation_list";
            var parameters = new DynamicParameters();
            parameters.Add("@flag", "s");
            parameters.Add("@userId", userId);
            parameters.Add("@reservationStatus", string.IsNullOrEmpty(reservationStatus) ? null : reservationStatus);
            parameters.Add("@pageNo", objPaginationRequestModel.pageNo);
            var dbResp = await _dao.ExecuteAsyncList<ReservationCommon>(proc, parameters);
            return dbResp;
        }
        public async Task<ReservationDetailCommon> GetReservationDetail(string userId, ReservationModel objReservationModel)
        {
            string proc = "apiproc_cp_reservation_details";
            var parameters = new DynamicParameters();
            parameters.Add("@flag", "rd");
            parameters.Add("@userId", userId);
            parameters.Add("@reservationId", objReservationModel.reservationId);
            var dbResp = await _dao.ExecuteAsync<ReservationDetailCommon>(proc, parameters);
            return dbResp;

        }
        //public async Task<List<HostDetailCommon>> GetHostList(string userId, ReservationDetailRequest objReservationDetailRequest)
        //{
        //    string proc = "apiproc_cp_reservation_list";
        //    var parameters = new DynamicParameters();
        //    parameters.Add("@flag", "rhd");
        //    parameters.Add("@userId", userId);
        //    parameters.Add("@reservationId", objReservationDetailRequest.reservationId);
        //    var dbResp = await _dao.ExecuteAsyncList<HostDetailCommon>(proc, parameters);
        //    return dbResp;
        //}
        public async Task<CommonDBResponse> CancelReservation(string userId, ReservationCancelModel objReservationCancelModel)
        {
            string proc = "apiproc_cp_reservation_cancel";
            var parameters = new DynamicParameters();
            parameters.Add("@flag", "cr");
            parameters.Add("@userId", userId);
            parameters.Add("@reservationId", objReservationCancelModel.reservationId);
            parameters.Add("@actionIp", objReservationCancelModel.actionIp);
            parameters.Add("@actionPlatform", objReservationCancelModel.actionPlatform);
            var dbResp = await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
            return dbResp;
        }
        public async Task<CommonDBResponse> DeleteReservation(string userId, ReservationCancelModel objReservationCancelModel)
        {
            string proc = "apiproc_cp_reservation_delete";
            var parameters = new DynamicParameters();
            parameters.Add("@flag", "dr");
            parameters.Add("@userId", userId);
            parameters.Add("@reservationId", objReservationCancelModel.reservationId);
            parameters.Add("@actionIp", objReservationCancelModel.actionIp);
            parameters.Add("@actionPlatform", objReservationCancelModel.actionPlatform);
            var dbResp = await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
            return dbResp;
        }
        public async Task<ReservationTimeDetail> GetReservationClubTime(string userId, ReservationModel objReservationModel)
        {
            string proc = "apiproc_cp_reservation_club_time";
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@reservationId", objReservationModel.reservationId);
            parameters.Add("@timeZoneOffsetValue", objReservationModel.timeZoneOffsetValue);
            var dbResp = await _dao.ExecuteAsync<ReservationTimeDetail>(proc, parameters);
            return dbResp;

        }
        public async Task<CommonDBResponse> UpdateReservationTime(string userId, ReservationTimeCommon objReservationTimelModel)
        {
            string proc = "apiproc_cp_reservation_update_time";
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@reservationId", objReservationTimelModel.reservationId);
            parameters.Add("@time", objReservationTimelModel.time);
            parameters.Add("@actionIp", objReservationTimelModel.actionIp);
            parameters.Add("@actionPlatform", objReservationTimelModel.actionPlatform);
            parameters.Add("@timeZoneOffsetValue", objReservationTimelModel.timeZoneOffsetValue);
            var dbResp = await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
            return dbResp;
        }

    }
}
