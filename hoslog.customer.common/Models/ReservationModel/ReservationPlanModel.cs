namespace hoslog.customer.common.Models.ReservationModel;

public class ReservationPlanListResponseModel
{
    public string? planId { get; set; }
    public string? planName { get; set; }
    public string? planNameEnglish { get; set; }
    public int planTime { get; set; }
    public int planPrice { get; set; }
    public bool isStrikeOut { get; set; }
    public int strikePrice { get; set; }
    public int planNomination { get; set; }
    public int noOfPeople { get; set; }
    public string? planRemarks { get; set; }
    public string? planLiquor { get; set; }
    public string? planLiquorJapanese { get; set; }
    public string? planType { get; set; }
    public bool hasMostPopularTag { get; set; }
}