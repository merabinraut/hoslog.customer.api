namespace hoslog.customer.shared.Recommendation
{
    public class RecommendedClubCommon
    {
        public int? groupClubCount { get; set; }
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public string? locationId { get; set; }
        public string? locationURL { get; set; }
        public string? clubName { get; set; }
        public string? clubNameJapanese { get; set; }
        public string? groupName { get; set; }
        public string? fullName { get; set; }
        public string? clubLogo { get; set; }
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
        public string? totalComment { get; set; }
        public string? averageRating { get; set; }
        public int? totalPages { get; set; }
        public string? clubCategory { get; set; }
        public string? landline { get; set; }
        public int positionCount { get; set; }
        public int totalClubPlanCount { get; set; }
        public List<HostImageForClubCardCommon> hostGalleryImage { get; set; } = new List<HostImageForClubCardCommon>();
    }
    public class HostImageForClubCardCommon
    {
        public string? clubId { get; set; }
        public string? hostId { get; set; }
        public string? hostCode { get; set; }
        public string? imagePath { get; set; }
    }

    public class RecommendedHostCommon
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
        public string? liquorStrength { get; set; }
        public string? dob { get; set; }
        public string? hostBirthPlace { get; set; }
        public int? loveCount { get; set; }
        public string? hostNameJapanese { get; set; }
        public string? formattedDOB { get; set; }
        public bool? isBookmarked { get; set; }
        public int? totalPages { get; set; }
        public int? groupClubCount { get; set; }
        public int positionCount { get; set; }
    }

    public class RecommendedClubPlanCommon
    {
        public string planId { get; set; } = null!;
        public string planName { get; set; } = null!;
        public string planTime { get; set; } = null!;
        public int planPrice { get; set; }
        public int strikePrice { get; set; }
        public bool isStrikeOut { get; set; }
        public int planNomination { get; set; }
        public string planRemarks { get; set; } = null!;
        public string planLiquor { get; set; } = null!;
        public string planLiquorJapanese { get; set; } = null!;
        public string openHours { get; set; } = null!;
        public string closeHours { get; set; } = null!;
        public int noOfPeople { get; set; }
        public bool hasMostPopularTag { get; set; }
        public string? planNameEnglish { get; set; }
    }

    public class EditorsRecommendationClubCommon
    {
        public string? locationId { get; set; }
        public string? clubCode { get; set; }
        public string? clubId { get; set; }
        public string? storeName { get; set; }
        public string? storeNameJp { get; set; }
        public string? storeImagePath { get; set; }
        public string? groupName { get; set; }
        public string? groupNameJp { get; set; }
        public int totalData { get; set; }
    }

    public class EditorsRecommendationClubDetailCommon : CommonDBResponse
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
        public int? totalData { get; set; }
        public List<HostImageForClubCardCommon> hostGalleryImage { get; set; } = new List<HostImageForClubCardCommon>();
    }
}