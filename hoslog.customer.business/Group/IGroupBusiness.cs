using System.Net;
using hoslog.customer.common.Models.Group;

namespace hoslog.customer.business.Group;

public interface IGroupBusiness
{
    Task<(HttpStatusCode, object)> GetGroupDetail(GroupRequestModel requestModel);
    Task<(HttpStatusCode, object)> GetGroupList(string freeWord = "", string vowel = "", int pageNo = 1);
}
