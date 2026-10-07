using hoslog.customer.shared;

namespace hoslog.customer.common.Models.Group;

public class GroupListResponse
{
    public PaginationResponse pagination { get; set; } = new PaginationResponse();
    public List<GroupInfoModel> groupList { get; set; } = new List<GroupInfoModel>();
}

public class GroupInfoModel : ImageResponseModel
{
    public string? groupName { get; set; }
    public string? groupId { get; set; }
    public string? groupNameKatakana { get; set; }
}
public class ImageResponseModel
{
    public ImageResponse? imageResponse { get; set; } = new ImageResponse();
}
public class GroupRequestModel
{
    public string? groupId { get; set; }
    public string? clubId { get; set; }
}

public class GroupDetailModel : GroupInfoModel
{
    public string? clubCode { get; set; }
}

public class GroupGalleryModel : ImageResponseModel
{
}
public class GroupedSubGroupClubModel
{
    public string? subGroupId { get; set; }
    public string? subGroupName { get; set; }
    public List<AssignedGroupClubModel>? clubs { get; set; }
}
public class AssignedGroupClubModel : ImageResponseModel
{
    public string? clubId { get; set; }
    public string? locationId { get; set; }
    public string? clubCode { get; set; }
    public string? subGroupId { get; set; }
    public string? subGroupName { get; set; }
    public string? clubName { get; set; }
    public string? clubNameJapanese { get; set; }
    public string? description { get; set; }
    public ImageResponse? resClubCoverPhoto { get; set; } = new ImageResponse();
    public List<ImageResponse>? hostImagesList { get; set; } = new List<ImageResponse>();
}
