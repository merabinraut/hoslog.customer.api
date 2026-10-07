using hoslog.customer.shared;

namespace hoslog.customer.common.Models.Search
{
    public class HostSearchResponseModel
    {
        public PaginationResponse pagination { get; set; } = new PaginationResponse();
        public int hostCount { get; set; }
        public List<HostDetailModel> hostDetail { get; set; } = new List<HostDetailModel>();
    }
    public class HostDetailModel
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
        public string? loveCount { get; set; }
        public string? hostNameJapanese { get; set; }
        public string? formattedDOB { get; set; }
        public bool isBookmarked { get; set; }
        public int? totalPages { get; set; }

        public ClubSearchLogoModel recommendationClubLogo { get; set; } = new ClubSearchLogoModel();
        public HostSearchLogoModel recommendationHostImage { get; set; } = new HostSearchLogoModel();

    }
    public class HostSearchLogoModel
    {
        public string? path { get; set; }
        public string? description { get; set; }
        public string? alt { get; set; }
    }
}