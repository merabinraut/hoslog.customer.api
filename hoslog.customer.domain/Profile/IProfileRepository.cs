using hoslog.customer.common.Models;
using hoslog.customer.shared;
using hoslog.customer.shared.Profile;

namespace hoslog.customer.domain.Profile
{
    public interface IProfileRepository
    {
        Task<List<ProfileCommon>> GetUserDetails(string userId);
        Task<CommonDBResponse> UpdateUserDetails(UserRequestCommon mappedRequest, string userId);
        Task<CommonDBResponse> SaveImage(string profileImage, string userId);
        Task<List<PhotoGalleryResponseCommon>> GetGalleryPhotoAsync(PhotoGalleryRequestCommon requestModel);
        Task<CommonDBResponse> DeleteUserAsync(CommonDBRequest request);
        Task<CommonDBResponse> DeleteUserAsyncWithoutTransaction(CommonDBRequest request);
    }
}
