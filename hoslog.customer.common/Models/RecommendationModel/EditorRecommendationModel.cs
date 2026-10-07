using System.Text.Json.Serialization;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.RecommendationModel;

public class EditorRecommendationModel
{
    public string? locationId { get; set; }
    public string? clubId { get; set; }
    public string? storeName { get; set; }
    public string? storeNameJp { get; set; }
    [JsonIgnore]
    public string? storeImagePath { get; set; }
    public ImageResponse? storeImage { get; set; }
    public string? groupName { get; set; }
    public string? groupNameJp { get; set; }
    
}

public class EditorRecommendationClubDetailListModel
{
    public string? clubId { get; set; }
    public string? clubCode { get; set; }
    public string? locationId { get; set; }
    public string? locationURL { get; set; }
    public string? clubName { get; set; }
    public string? clubNameJapanese { get; set; }
    public string? groupName { get; set; }
    public string? fullName { get; set; }
    [JsonIgnore]
    public string? clubLogo { get; set; }
    public string? editorsDescription { get; set; }
    public string? editorsTags { get; set; }
    public string? clubOpeningTime { get; set; }
    public string? clubClosingTime { get; set; }
    public string? clubTimePeriod { get; set; }
    public string? todaysClubSchedule { get; set; }
    public bool? isBookmarked { get; set; }
    public string? totalComment { get; set; }
    public string? averageRating { get; set; }
    public int? totalHosts { get; set; }
    public ImageResponse? recommendationClubLogo { get; set; } = new ImageResponse();
    public List<HostImageForClubCardModel> hostGalleryImage { get; set; } = new List<HostImageForClubCardModel>();
}
