using Dapper;
using hoslog.customer.shared.Group;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Group;

public class GroupRepository : IGroupRepository
{
    private readonly RepositoryDao _dao;
    public GroupRepository(IConfiguration configuration)
    {
        _dao = new RepositoryDao(configuration);
    }

    public async Task<IList<GroupDetailCommon>> GetGroupDetailViaId(string groupId, string clubId)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_group_detail]";
        param.Add("@groupId", !string.IsNullOrEmpty(groupId) ? groupId : null);
        param.Add("@clubId", clubId);
        var dbResponse = await _dao.ExecuteAsyncList<GroupDetailCommon>(sp_name, param);
        return dbResponse;
    }

    public async Task<IList<GroupInfoCommon>> GetGroupList(string freeWord = "", string vowel = "", int pageNo = 1)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_group_list]";
        param.Add("@searchText", !string.IsNullOrEmpty(freeWord) ? freeWord : null);
        param.Add("@hiraganaStartVowel", !string.IsNullOrEmpty(vowel) ? vowel : null);
        param.Add("@pageNo", pageNo);
        var dbResponse = await _dao.ExecuteAsyncList<GroupInfoCommon>(sp_name, param);
        return dbResponse;
    }

    public async Task<List<AssignedGroupClubCommon>> GetSubGroupAndClubListViaGroupId(string groupId)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_sub_group_club]";
        param.Add("@groupId", groupId);
        var dbResponse = await _dao.ExecuteAsyncList<AssignedGroupClubCommon>(sp_name, param);
        return dbResponse;
    }
    public async Task<List<AssignedGroupClubCommon>> GetSubGroupAndClubListViaClubId(string clubId)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_sub_group_club_via_club_id]";
        param.Add("@clubId", clubId);
        var dbResponse = await _dao.ExecuteAsyncList<AssignedGroupClubCommon>(sp_name, param);
        return dbResponse;

    }

    public async Task<IList<GroupGalleryCommon>> GroupGalleryList(string groupId)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_group_gallery]";
        param.Add("@groupId", groupId);
        var dbResponse = await _dao.ExecuteAsyncList<GroupGalleryCommon>(sp_name, param);
        return dbResponse;
    }
}
