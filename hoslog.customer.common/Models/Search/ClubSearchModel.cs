using hoslog.customer.common.Models.RecommendationModel;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.Search
{
    public class ClubSearchRequestModel
    {
        public string? locationId { get; set; }
        public string? freeWord { get; set; }
        public string? selectedDate { get; set; }
        public string? selectedTime { get; set; }
        public int noOfPeople { get; set; }
        public string? planId { get; set; }
        public string? businessHours { get; set; }
        public string? tags { get; set; }
        public int pageNo { get; set; }
        public string? customerId { get; set; }
    }

    public class ClubSearchResponseModel
    {
        public PaginationResponse pagination { get; set; } = new PaginationResponse();
        public int clubCount { get; set; }
        public List<ClubSearchDetailModel> clubList { get; set; } = new List<ClubSearchDetailModel>();
    }


    public class ClubSearchDetailModel
    {
        public string clubId { get; set; } = null!;
        public string clubCode { get; set; } = null!;
        public string locationId { get; set; } = null!;
        public string locationURL { get; set; } = null!;
        public string clubName { get; set; } = null!;
        public string clubNameJapanese { get; set; } = null!;
        public string groupName { get; set; } = null!;
        public string fullName { get; set; } = null!;
        // public string clubLogo { get; set; } = null!;
        public string clubDescription { get; set; } = null!;
        public string tag1 { get; set; } = null!;
        public string tag2 { get; set; } = null!;
        public string tag3 { get; set; } = null!;
        public string tag4 { get; set; } = null!;
        public string tag5 { get; set; } = null!;
        public string clubOpeningTime { get; set; } = null!;
        public string clubClosingTime { get; set; } = null!;
        public string clubTimePeriod { get; set; } = null!;
        public string todaysClubSchedule { get; set; } = null!;
        public bool isBookmarked { get; set; }
        public string totalComment { get; set; } = null!;
        public string averageRating { get; set; } = null!;
        public string clubCategory { get; set; } = null!;
        public string landline { get; set; } = null!;
        public int remainingSlot { get; set; }
        public int totalClubPlanCount { get; set; }
        public ClubSearchLogoModel recommendationClubLogo { get; set; } = new ClubSearchLogoModel();
        public ClubSearchPlanModel plans { get; set; } = new ClubSearchPlanModel();
        public List<HostImageForClubCard> hostGalleryImage { get; set; } = new List<HostImageForClubCard>();
    }

    public class ClubSearchPlanModel
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

    public class ClubSearchLogoModel
    {
        public string alt { get; set; } = null!;
        public string description { get; set; } = null!;
        public string path { get; set; } = null!;
    }
}