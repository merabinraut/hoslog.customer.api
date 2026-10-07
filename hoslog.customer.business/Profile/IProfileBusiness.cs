using hoslog.customer.shared;
using hoslog.customer.shared.Profile;
using System.Net;

namespace hoslog.customer.business.Profile
{
    public interface IProfileBusiness
    {
        Task<(HttpStatusCode, object)> GetUserDetails(string userId, AmazonSettingsCommon common);
        Task<(HttpStatusCode, object)> UpdateUserDetails(UserRequestCommon UserRequestCommon, AmazonSettingsCommon common);
        Task<(HttpStatusCode, object)> SaveImage(string profileImage, string userId);
        Task<(HttpStatusCode, object)> GetGalleryPhotoAsync(string clubId, string clubCode, int pageNo = 1);
        Task<(HttpStatusCode, object)> DeleteUserAsync(string userId);
    }
}
