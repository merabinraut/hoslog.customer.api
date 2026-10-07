using hoslog.customer.shared;

namespace hoslog.customer.common.Models;

public class RecommendationHostModelCommon : CommonApiResponse
{
    public string? hostId { get; set; }
    public string? hostName { get; set; }
    public string? hostNameEng { get; set; }
    public string? clubId { get; set; }
    public string? clubName { get; set; }
    public string? clubNameEng { get; set; }
    public string? hostImage { get; set; }
    public string? locationId { get; set; }
    public bool? isBookmarked { get; set; }
    public string? clubLogo { get; set; }
    
}
public class RecommendationHostModel : RecommendationHostModelCommon
{
    public ImageResponse? recommendedHostImage { get; set; } = new ImageResponse();
    public ImageResponse? recommendedClubLogo { get; set; } = new ImageResponse();
}
public class RecommendationClubModelCommon : CommonApiResponse
{
    public string? clubId { get; set; }
    public string? clubName { get; set; }
    public string? clubNameEng { get; set; }
    public string? locationId { get; set; }
    public string? clubLogo { get; set; }
    public bool? isBookmarked { get; set; }
    public string? clubCategory { get; set; }
    public string? landline { get; set; }
}
public class RecommendationClubModel : RecommendationClubModelCommon
{
    public ImageResponse? recommendedClubLogo { get; set; } = new ImageResponse();

}


