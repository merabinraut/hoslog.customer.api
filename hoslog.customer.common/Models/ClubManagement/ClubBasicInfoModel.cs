using hoslog.customer.shared;

namespace hoslog.customer.common.Models.ClubManagement
{
    public class ClubBasicInformationResponseModel : ClubUrlLinkModel
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
        public int? hostCount { get; set; }
        public int? galleryImageCount { get; set; }
        public int? reviewCount { get; set; }
        public string? clubLocationName { get; set; }
        public bool canReserve { get; set; } = false;
        public ImageResponse? resClubLogo { get; set; } = new ImageResponse();
        public ImageResponse? resClubCoverPhoto { get; set; } = new ImageResponse();
        public LocationResponse? location { get; set; } = new LocationResponse();
    }
}
