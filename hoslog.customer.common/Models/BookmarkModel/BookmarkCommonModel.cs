using System;
using FluentValidation;
using hoslog.customer.common.Models.RecommendationModel;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.BookmarkModel;

public class BookmarkCommonModel : CommonDTO
{
    public string? agentId { get; set; }
    public string? hostId { get; set; }
    public string? type { get; set; }
    public string? userId { get; set; }

}
public class BookmarkTabList
{
    public IList<BookmarkedClubModel>? BookmarkedClubs { get; set; } = null;
    public IList<BookmarkedHostModel>? BookmarkedHosts { get; set; } = null;
}
public class BookmarkedHostCommonModel
{
    public string? locationId { get; set; }
    public string? locationURL { get; set; }
    public string? clubId { get; set; }
    public string? clubCode { get; set; }
    public string? hostId { get; set; }
    public string? hostCode { get; set; }
    public string? hostName { get; set; }
    public string? previousOccupation { get; set; }
    public string? rank { get; set; }
    public string? clubName { get; set; }
    public string? clubNameJap { get; set; }
    public string? clubLogo { get; set; }
    public string? hostImage { get; set; }
    public string? constellationGroup { get; set; }
    public string? height { get; set; }
    public string? bloodType { get; set; }
    // public string? liquorStrength { get; set; }
    public string? dob { get; set; }
    public string? address { get; set; }
    public string? loveCount { get; set; }
    public string? hostNameJapanese { get; set; }
    public string? formattedDOB { get; set; }
    public string? mbti { get; set; }
    public int hostReviews { get; set; } = 0;
}
public class BookmarkedHostModel : BookmarkedHostCommonModel
{
    public ImageResponse? hostBookmarkedClubLogo { get; set; } = new ImageResponse();
    public ImageResponse? hostBookmerkedHostImage { get; set; } = new ImageResponse();
}
public class BookmarkedClubCommonModel
{
    public string? clubId { get; set; }
    public string? clubCode { get; set; }
    public string? locationId { get; set; }
    public string? locationURL { get; set; }
    public string? clubName { get; set; }
    public string? clubNameJapanese { get; set; }
    public string? groupName { get; set; }
    public string? fullName { get; set; }
    public string? clubLogo { get; set; }
    public string? clubCoverPhoto { get; set; }
    public string? clubDescription { get; set; }
    public string? tag1 { get; set; }
    public string? tag2 { get; set; }
    public string? tag3 { get; set; }
    public string? tag4 { get; set; }
    public string? tag5 { get; set; }
    public string? clubOpeningTime { get; set; }
    public string? clubClosingTime { get; set; }
    public string? clubTimePeriod { get; set; }
    public string? todaysClubSchedule { get; set; }
    public bool? isBookmarked { get; set; }
    public int? totalComment { get; set; }
    public int? averageRating { get; set; }
    public string? clubCategory { get; set; }
    public List<HostImageForClubCard>? clubHostImages { get; set; } = new List<HostImageForClubCard>();
    public string? landline { get; set; }
    public RecommendedClubPlanModel plans { get; set; } = new RecommendedClubPlanModel();
    public int totalClubPlanCount { get; set; }
}
public class BookmarkedClubModel : BookmarkedClubCommonModel
{
    public ImageResponse? clubBookmarkedClublogo { get; set; } = new ImageResponse();
    public ImageResponse? clubBookmarkedClubCoverImage { get; set; } = new ImageResponse();
}















