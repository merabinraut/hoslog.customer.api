using hoslog.customer.common.Models;
using hoslog.customer.common.Models.Reservation;
using hoslog.customer.shared;
using hoslog.customer.shared.Reservation;

namespace hoslog.customer.domain.Reservation
{
    public interface IReservationHistoryRepository
    {
        Task<List<ReservationCommon>> GetReservationList(PaginationRequestModel objPaginationRequestModel, string userId, string reservationStatus = "");
        Task<ReservationDetailCommon> GetReservationDetail(string userId, ReservationModel objReservationModel);
        //Task<List<HostDetailCommon>> GetHostList(string userId, ReservationDetailRequest objReservationDetailRequest);
        Task<CommonDBResponse> CancelReservation(string userId, ReservationCancelModel objReservationCancelModel);
        Task<CommonDBResponse> DeleteReservation(string userId, ReservationCancelModel objReservationCancelModel);
        Task<ReservationTimeDetail> GetReservationClubTime(string userId, ReservationModel objReservationModel);
        Task<CommonDBResponse> UpdateReservationTime(string userId, ReservationTimeCommon objReservationTimelModel);
    }
}
