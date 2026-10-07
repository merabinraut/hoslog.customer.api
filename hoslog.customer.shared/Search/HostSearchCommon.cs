namespace hoslog.customer.shared.Search
{
    public class HostSearchRequestCommon
    {
        public string? locationId { get; set; }
        public string? freeWord { get; set; }
        public int? heightMin { get; set; }
        public int? heightMax { get; set; }
        public int? ageMin { get; set; }
        public int? ageMax { get; set; }
        public string? bloodType { get; set; }
        public string? constellation { get; set; }
        public string? previousJob { get; set; }
        public string? role { get; set; }
        public string? customerId { get; set; }
        public string? vowel { get; set; }
        public int? pageNo { get; set; }
    }

    public class HostSearchResponseCommon : PaginationResponse
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
        public string? clubLogoPath { get; set; }
        public string? clubLogoDescription { get; set; }
        public string? clubLogoAlt { get; set; }
        public string? hostLogoPath { get; set; }
        public string? hostLogoDescription { get; set; }
        public string? hostLogoAlt { get; set; }
    }
}