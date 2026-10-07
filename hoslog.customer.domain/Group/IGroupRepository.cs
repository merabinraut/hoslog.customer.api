using hoslog.customer.shared.Group;

namespace hoslog.customer.domain.Group;

public interface IGroupRepository
{
    Task<IList<GroupDetailCommon>> GetGroupDetailViaId(string groupId, string clubId);
    Task<IList<GroupInfoCommon>> GetGroupList(string freeWord = "", string vowel = "", int pageNo = 1);
    Task<List<AssignedGroupClubCommon>> GetSubGroupAndClubListViaClubId(string clubId);
    Task<List<AssignedGroupClubCommon>> GetSubGroupAndClubListViaGroupId(string groupId);
    Task<IList<GroupGalleryCommon>> GroupGalleryList(string groupId);
}
