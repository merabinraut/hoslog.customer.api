using hoslog.customer.common.Models;
using hoslog.customer.common.Models.Reservation;
using hoslog.customer.shared;
using System.Net;

namespace hoslog.customer.business.Reservation
{
    public interface IReservationHistoryBusiness
    {
        Task<(HttpStatusCode, object)> GetReservationList(PaginationRequestModel objPaginationRequestModel, AmazonSettingsCommon common, string userId, string reservationStatus = "");
        Task<(HttpStatusCode, object)> GetReservationDetail(AmazonSettingsCommon common, string userId, ReservationModel objReservationModel);
        Task<(HttpStatusCode, object)> CancelReservation(ReservationCancelModel objReservationCancelModel, string userId);
        Task<(HttpStatusCode, object)> DeleteReservation(ReservationCancelModel objReservationCancelModel, string userId);
        Task<(HttpStatusCode, object)> GetReservationClubTime(string userId, ReservationModel objReservationModel);
        Task<(HttpStatusCode, object)> UpdateReservationTime(ReservationTimelModel objReservationTimelModel, string userId);
    }
}
