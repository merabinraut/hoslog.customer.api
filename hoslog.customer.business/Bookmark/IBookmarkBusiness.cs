using System.Net;
using hoslog.customer.shared;

namespace hoslog.customer.business.Bookmark;

public interface IBookmarkBusiness
{
    Task<(HttpStatusCode statusCode, object response)> GetBookmarkHostAndClubList(AmazonSettingsCommon awsSettingCommon);
    Task<(HttpStatusCode, object)> ManageBookmark(string agentId = "", string hostId = "", string type = "");
}
