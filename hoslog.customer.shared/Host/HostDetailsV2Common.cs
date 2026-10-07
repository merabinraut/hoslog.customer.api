namespace hoslog.customer.shared.Host;

public class HostDetailsV2Common : CommonDBResponse
{
    public string? clubId { get; set; }
    public string? clubName { get; set; }
    public string? clubNameKatakana { get; set; }
    public bool? clubLocationId { get; set; }
    public string? clubLocationName { get; set; }
    public string? clubLocationURL { get; set; }
    public string? clubCode { get; set; }
    public string? clubLogo { get; set; }
    public string? hostId { get; set; }
    public string? hostCode { get; set; }
    public int hostRank { get; set; }
    public string? hostName { get; set; }
    public string? hostNameJapanese { get; set; }
    public string? hostRole { get; set; }
    public string? hostBadge { get; set; }
    public string? hostWebsiteLink { get; set; }
    public string? hostTiktokLink { get; set; }
    public string? hostTwitterLink { get; set; }
    public string? hostInstagramLink { get; set; }
    public string? hostFacebookLink { get; set; }
    public string? hostLineLink { get; set; }
    public string? hostHeight { get; set; }
    public string? hostBirthPlace { get; set; }
    public string? hostDOB { get; set; }
    public string? DOB { get; set; }
    public string? hostAge { get; set; }
    public string? hostBloodType { get; set; }
    public string? hostZodiac { get; set; }
    public string? appearance { get; set; }
    public string? conversationStyle { get; set; }
    public string? alcoholTolerance { get; set; }
    public string? atmosphere { get; set; }
    public bool isHostBookmarked { get; set; } = false;
    public int hostLikes { get; set; } = 0;
    public int hostReviews { get; set; } = 0;
    public string? hostMbtiTypeCode { get; set; }
    public string? locationMappingURL { get; set; }
}

public class HostImagesV2Common : CommonDBResponse
{
    public string? id { get; set; }
    public string? imagePath { get; set; }
}