namespace hoslog.customer.shared.Reservation
{
    public class ReservationCommon : PaginationResponse
    {
        public string? reservationId { get; set; }
        public string? locationId { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public long? reservedDate { get; set; }
        public long? visitDateTime { get; set; }
        public string? invoiceId { get; set; }
        public string? clubNameEng { get; set; }
        public string? clubName { get; set; }
        public string? groupNameEng { get; set; }
        public string? groupName { get; set; }
        public string? mobileNumber { get; set; }
        public string? price { get; set; }
        public string? clubLogo { get; set; }
        public string? locationURL { get; set; }
        public string? noOfPeople { get; set; }
        public string? tag1 { get; set; }
        public string? tag2 { get; set; }
        public string? tag3 { get; set; }
        public string? tag4 { get; set; }
        public string? tag5 { get; set; }
        public string? status { get; set; }
        public string? latitude { get; set; }
        public string? longitude { get; set; }
        public string? canChangeTime { get; set; }
        public string? canCancel { get; set; }
        public string? canRedo { get; set; }
        public string? canDelete { get; set; }
    }
    public class ReservationCommonData
    {
        public PaginationResponse pagination { get; set; } = new PaginationResponse();
        public List<ReservationCommonResponse> reservationList { get; set; } = new List<ReservationCommonResponse>();
    }
    public class ReservationCommonResponse
    {
        public string? reservationId { get; set; }
        public string? locationId { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public long? reservedDate { get; set; }
        public long? visitDateTime { get; set; }
        public string? invoiceId { get; set; }
        public string? clubNameEng { get; set; }
        public string? clubName { get; set; }
        public string? groupNameEng { get; set; }
        public string? groupName { get; set; }
        public string? mobileNumber { get; set; }
        public int? price { get; set; }
        public int? noOfPeople { get; set; }
        public string? tag1 { get; set; }
        public string? tag2 { get; set; }
        public string? tag3 { get; set; }
        public string? tag4 { get; set; }
        public string? tag5 { get; set; }
        public string? status { get; set; }
        public Boolean? canChangeTime { get; set; }
        public Boolean? canCancel { get; set; }
        public Boolean? canRedo { get; set; }
        public Boolean? canDelete { get; set; }
        public ImageResponse? image { get; set; }
        public LocationResponse? location { get; set; }
    }

    #region reservationdetails
    public class ReservationDetailRequest
    {
        public string? reservationId { get; set; }
    }
    public class ReservationDetailCommon
    {
        public string? reservationId { get; set; }
        public string? customerId { get; set; }
        public long? reservedDate { get; set; }
        public long? visitDateTime { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public string? clubNameEng { get; set; }
        public string? clubName { get; set; }
        public string? mobileNumber { get; set; }
        public string? clubAddress { get; set; }
        public string? visitCode { get; set; }
        public string? clubLogo { get; set; }
        public string? locationName { get; set; }
        // public string? visitDate { get; set; }
        // public string? visitTime { get; set; }
        public string? planName { get; set; }
        public string? price { get; set; }
        public string? planTime { get; set; }
        public string? noOfPeople { get; set; }
        public string? liquor { get; set; }
        public string? nomination { get; set; }
        public string? isPopularity { get; set; }
        public object? hostDetails { get; set; }

    }
    public class HostDetailCommon
    {
        public string? hostName { get; set; }
        public string? hostId { get; set; }
        public string? image { get; set; }

    }
    public class ReservationDetailResponse
    {
        public string? reservationId { get; set; }
        public string? customerId { get; set; }
        public long? reservedDate { get; set; }
        public long? visitDateTime { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public string? clubNameEng { get; set; }
        public string? clubName { get; set; }
        public string? clubAddress { get; set; }
        public string? visitCode { get; set; }
        public string? mobileNumber { get; set; }
        public ImageResponse? clubLogo { get; set; }
        public string? locationName { get; set; }
        public long? visitDate { get; set; }
        public string? visitTime { get; set; }
        public PlanDetailResponse? planDetail { get; set; }
        public List<HostDetailResponse>? hostDetails { get; set; }
    }
    public class PlanDetailResponse
    {
        public string? planName { get; set; }
        public int? price { get; set; }
        public string? planTime { get; set; }
        public int? noOfPeople { get; set; }
        public string? liquor { get; set; }
        public int? nomination { get; set; }
        public Boolean? isPopularity { get; set; }
    }
    public class HostDetailResponse
    {
        public string? hostId { get; set; }
        public ImageResponse? image { get; set; }
    }
    #endregion
    #region Time List

    public class ReservationTimeDetailResponse
    {
        public string? visitTime { get; set; }
        public int? noOfPeople { get; set; }
        public string? reservationId { get; set; }
        public List<TimeDetail>? reservationTimeList { get; set; }
    }
    public class ReservationTimeDetail : CommonDBResponse
    {
        public string? visitTime { get; set; }
        public string? noOfPeople { get; set; }
        public string? reservationId { get; set; }
        public object? clubTimeList { get; set; }
    }
    public class TimeDetail
    {
        public Boolean? isAvailable { get; set; }
        public string? time { get; set; }

    }

    public class ClubReservationTimeRequestCommon : CommonDBRequest
    {
        public string clubId { get; set; } = null!;
        public long? date { get; set; }
    }

    public class ClubReservationTimeResponseCommon : CommonDBResponse
    {
        public long date { get; set; }
        public string clubOpeningTime { get; set; } = null!;
        public string clubClosingTime { get; set; } = null!;
        public string lastEntryTime { get; set; } = null!;
        public string time { get; set; } = null!;
        public string status { get; set; } = null!;
    }
    #endregion
    public class ReservationTimeCommon : CommonDBRequest
    {
        public string? reservationId { get; set; }
        public long? time { get; set; }
        public string? actionIp { get; set; }
    }
}
