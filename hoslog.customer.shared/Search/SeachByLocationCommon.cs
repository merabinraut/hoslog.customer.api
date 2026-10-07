using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.shared.Search
{
    public class SeachByLocationReponseCommon : CommonDBResponse
    {
        public string? locationUrl { get; set; }
    }
    #region Club search details
    public class ClubListCommon
    {
        public string? clubId { get; set; }
        public string? ClubLocationId {  get; set; }
        public string? clubNameEnglish {  get; set; }
        public string? clubNameJapanese {  get; set; }
        public string? groupName {  get; set; }
        public string? fullName {  get; set; }
        public string? clubLogo {  get; set; }
        public string? clubCoverPhoto {  get; set; }
        public string? clubDescription {  get; set; }
        public string? tag1 {  get; set; }
        public string? tag2 {  get; set; }
        public string? tag3 {  get; set; }
        public string? tag4 {  get; set; }
        public string? tag5 {  get; set; }
        public string? clubOpeningTime {  get; set; }
        public string? clubClosingTime {  get; set; }
        public string? clubTimePeriod {  get; set; }
        public string? todaysClubSchedule {  get; set; }
        public string? locationURL {  get; set; }
        public string? sno {  get; set; }
        public string? userId {  get; set; }
        public string? roleId {  get; set; }
        public string? agentId {  get; set; }
        public string? status {  get; set; }
        public string? isPrimary {  get; set; }
        public string? isBookmarked { get; set; }
        public int totalComment { get; set; }
        public int averageRating { get; set; }
        public string? imagePath { get; set; }
        public string? hostId { get; set; }
        public string? clubCategory { get; set; }
    }
    public class HostImageListCommon
    {
        public string? imagePath { get; set; }
        public string? hostId { get; set; }            
    }
    #endregion
    #region Host details
    public class HostDetails
    { 
        public string? hostName { get; set; }
        public string? clubNamme { get; set; }
        public string? hostId { get; set; }
        public string? clubId { get; set; }
        public string? hostImage { get; set; }
        public string? position { get; set; }
    }
    public class HostDetailsResponseCommon
    {
        public string? hostName { get; set; }
        public string? hostNameEng { get; set; }
        public string? clubNamme { get; set; }
        public string? hostId { get; set; }
        public string? clubId { get; set; }
        public string? position { get; set; }
        public string? otherPosition { get; set; }
        public string? totalCount { get; set; }
        public string? clubLogo { get; set; }
        public string? clubCoverPhoto { get; set; }
        public string? clubBusinessCertificate { get; set; }
        public string? hostThumbnail { get; set; }
        public string? hostIcon { get; set; }
        public string? hostImagePath { get; set; }
        public string? rowsTotal { get; set; }
        public string? rowNum { get; set; }
        public string? clubCategory { get; set; }
    }
    #endregion

}
