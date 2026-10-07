namespace hoslog.customer.shared.Group;

public class GroupInfoCommon : PaginationResponse
{
    public string? groupName { get; set; }
    public string? groupId { get; set; }
    public string? groupNameKatakana { get; set; }
    public string? image { get; set; }
}
public class GroupDetailCommon : GroupInfoCommon
{
    public string? clubCode { get; set; }
}
public class GroupGalleryCommon
{
    public string? image { get; set; }
}
public class SubGroupCommon
{
    public string? subGroupId { get; set; }
    public string? subGroupName { get; set; }
    //public List<AssignedGroupClubCommon> GetAssignedGroupClub { get; set; } = new List<AssignedGroupClubCommon>();
}
public class AssignedGroupClubCommon
{
    public string? clubId { get; set; }
    public string? locationId { get; set; }
    public string? clubCode { get; set; }
    public string? subGroupId { get; set; }
    public string? subGroupName { get; set; }
    public string? clubName { get; set; }
    public string? clubNameJapanese { get; set; }
    public string? description { get; set; }
    public string? clubLogo { get; set; }
    public string? hostImages { get; set; }
    public string? clubCoverPhoto { get; set; }
}
