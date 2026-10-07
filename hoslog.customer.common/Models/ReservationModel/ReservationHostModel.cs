using hoslog.customer.shared;

namespace hoslog.customer.common.Models.ReservationModel;

public class ReservationHostListResponseModel
{
    public string? hostId { get; set; }
    public string? hostNameEnglish { get; set; }
    public string? hostNameJapanese { get; set; }
    public string? hostPosition { get; set; }
    public int rank { get; set; }
    public ImageResponse? hostImageResponse { get; set; } = new ImageResponse();
}