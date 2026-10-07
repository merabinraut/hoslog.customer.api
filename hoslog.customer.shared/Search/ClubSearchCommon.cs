namespace hoslog.customer.shared.Search
{
    public class ClubSearchRequestCommon : CommonDBRequest
    {
        public string? locationId { get; set; }
        public string? freeWord { get; set; }
        public string? selectedDate { get; set; }
        public string? selectedTime { get; set; }
        public int noOfPeople { get; set; }
        public string? planId { get; set; }
        public string? businessHours { get; set; }
        public string? tags { get; set; }
        public string? customerId { get; set; }
        public string? vowel { get; set; }
        public int pageNo { get; set; }
        public bool canVisitStoreNow { get; set; } = false;
        public bool useFreePlanOnly { get; set; } = false;
        public int? minPlanPrice { get; set; }
        public int? maxPlanPrice { get; set; }
        public int? planTime { get; set; }
    }

    public class ClubSearchResponseCommon : PaginationResponse
    {
        public string clubId { get; set; } = null!;
        public string clubCode { get; set; } = null!;
        public string locationId { get; set; } = null!;
        public string locationURL { get; set; } = null!;
        public string clubName { get; set; } = null!;
        public string clubNameJapanese { get; set; } = null!;
        public string groupName { get; set; } = null!;
        // public string fullName { get; set; } = null!;
        // public string clubLogo { get; set; } = null!;
        public string clubLogoPath { get; set; } = null!;
        public string clubLogoDescription { get; set; } = null!;
        public string clubLogoAlt { get; set; } = null!;
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
        public string hostImages { get; set; } = null!;
        public int totalPages { get; set; }
        public int clubCount { get; set; }
        public int totalClubPlanCount { get; set; }
    }
    public class club
    {
        public int clubCount { get; set; }
    }

    public class ClubSearchPlanCommon
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
    }
}