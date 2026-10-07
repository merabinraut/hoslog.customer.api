namespace hoslog.customer.shared.Multitenant.ClubInformationMultitenantCommon
{
    public class ClubBasicInformationMultitenantCommon : ClubUrlLinkMultitenantCommon
    {
        public string? clubId { get; set; }
        public string? clubCode { get; set; }
        public string? locationId { get; set; }
        public string? clubNameEng { get; set; }
        public string? clubName { get; set; }
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
        public string? clubAddress { get; set; }
        public string? clubClosingTime { get; set; }
        public string? clubOpeningTime { get; set; }
        public string? clubTimePeriod { get; set; }
        public string? todaysClubSchedule { get; set; }
        public string? companionFee { get; set; }
        public string? designationFee { get; set; }
        public string? drinkFee { get; set; }
        public string? epLastEntryTime { get; set; }
        public string? extensionFee { get; set; }
        public string? lastEntryTime { get; set; }
        public string? lastOrderTime { get; set; }
        public string? lineURL { get; set; }
        public string? regularyEntry { get; set; }
        public string? regularPrice { get; set; }
        public string? tax { get; set; }
        public string? closingDate { get; set; }
        public string? clubHoliday { get; set; }
        public bool? isBookmarked { get; set; }
        public string? landlineNumber { get; set; }
        public string? totalComment { get; set; }
        public string? averageRating { get; set; }
        public string? mobileNumber { get; set; }
        public string? otherHoliday { get; set; }
        public string? clubCategory { get; set; }
        public string? locationURL { get; set; }
        public string? latitude { get; set; }
        public string? longitude { get; set; }
        public string? hostCount { get; set; }
        public string? galleryImageCount { get; set; }
        public string? reviewCount { get; set; }
        public string? clubLocationName { get; set; }
        public ImageResponse? resClubLogo { get; set; } = new ImageResponse();
        public ImageResponse? resClubCoverPhoto { get; set; } = new ImageResponse();
    }
    public class ClubUrlLinkMultitenantCommon
    {
        public string? tiktokURL { get; set; }
        public string? twitterURL { get; set; }
        public string? websiteURL { get; set; }
        public string? instagramURL { get; set; }
    }

    public class ClubPlanDetailMultitenantCommon
    {
        public string? label { get; set; }
        public string? labelValue { get; set; }
        public string? planName { get; set; }
        public string? clubPlanTypeId { get; set; }
        public string? planListId { get; set; }
        public string? clubId { get; set; }
    }
}