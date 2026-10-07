namespace hoslog.customer.shared.ReservationModel;

public class ReservationHostListResponseCommon : CommonDBResponse
{
    public string? hostId { get; set; }
    public string? hostNameEnglish { get; set; }
    public string? hostNameJapanese { get; set; }
    public string? hostImage { get; set; }
    public string? hostPosition { get; set; }
    public int rank { get; set; }
}