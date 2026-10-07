using hoslog.customer.shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace hoslog.customer.common.Models.Search
{
    public class ClubListResponseModel
    {
        public string? clubId { get; set; }
        public string? clubLocationId { get; set; }
        public string? clubName { get; set; }
        public string? engClubName { get; set; }
        public string? groupName { get; set; }
        //public string? fullName { get; set; }
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
        public string? locationURL { get; set; }
        public string? clubCategory { get; set; }
        //public string? sno { get; set; }
        //public string? userId { get; set; }
        //public string? roleId { get; set; }
        //public string? agentId { get; set; }
        //public string? status { get; set; }
        //public string? isPrimary { get; set; }
        public string? isBookmarked { get; set; }
        public int totalComment { get; set; }
        public int averageRating { get; set; }
        [JsonIgnore]
        public string? hostId { get; set; }
        public ImageResponse clubLogo { get; set; } = new ImageResponse();
        public ImageResponse clubCoverPhoto { get; set; } = new ImageResponse();
        public ImageResponse imagePath { get; set; } = new ImageResponse();
    }
    public class HostDetailsResponseModel
    {
        public string? hostName { get; set; }
        public string? hostNameEng { get; set; }
        public string? clubName { get; set; }
        public string? hostId { get; set; }
        public string? clubId { get; set; }
        public string? position { get; set; }
        public string? otherPosition { get; set; }
        public string? clubCategory { get; set; }
        //public string? totalCount { get; set; }
        public ImageResponse hostThumbnail {  get; set; } = new ImageResponse();
        public ImageResponse hostIcon {  get; set; } = new ImageResponse();
        public ImageResponse hostImagePath {  get; set; } = new ImageResponse();
        public ImageResponse clubLogo {  get; set; } = new ImageResponse();
        public ImageResponse clubCoverPhoto {  get; set; } = new ImageResponse();
        public ImageResponse clubBusinessCertificate {  get; set; } = new ImageResponse();
        public string? rowsTotal { get; set; }
        public string? rowNum { get; set; }

    }
}
