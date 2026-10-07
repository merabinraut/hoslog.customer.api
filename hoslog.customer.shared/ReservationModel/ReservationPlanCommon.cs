using hoslog.customer.shared;

namespace hoslog.customer.shared.ReservationModel;

public class ReservationPlanListRequestCommon : CommonDBRequest
{
    public long clubId { get; set; }
    public string? clubCode { get; set; }
    public string? planId { get; set; }
}

public class ReservationPlanListResponseCommon : CommonDBResponse
{
    public bool canReserve { get; set; } = true;
    public long previousVisitDate { get; set; }
    public long nextEligibleDate { get; set; }
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