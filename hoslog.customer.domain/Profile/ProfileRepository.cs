using Dapper;
using hoslog.customer.shared;
using Microsoft.Extensions.Configuration;
using hoslog.customer.shared.Profile;
using hoslog.customer.common.Models;
using hoslog.customer.common.Utilities;

namespace hoslog.customer.domain.Profile
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly RepositoryDao _dao;
        private readonly SqlTransactionHandler _sqlTransactionHandler;
        public ProfileRepository(IConfiguration configuration, SqlTransactionHandler sqlTransactionHandler)
        {
            _dao = new RepositoryDao(configuration);
            _sqlTransactionHandler = sqlTransactionHandler;

        }
        public async Task<List<ProfileCommon>> GetUserDetails(string userId)
        {
            string proc = "apiproc_cp_profile_detail";
            var parameters = new DynamicParameters();
            parameters.Add("@flag", "s");
            parameters.Add("@userId", userId);
            var dbResp = await _dao.ExecuteAsyncList<ProfileCommon>(proc, parameters);
            return dbResp;
        }
        public async Task<CommonDBResponse> UpdateUserDetails(UserRequestCommon mappedRequest, string userId)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_cp_profile_detail";
            parameters.Add("@flag", "u");
            parameters.Add("@firstName", mappedRequest.firstName);
            parameters.Add("@lastName", mappedRequest.lastName);
            parameters.Add("@dob", mappedRequest.dob);
            parameters.Add("@emailAddress", mappedRequest.email);
            parameters.Add("@gender", mappedRequest.gender);
            parameters.Add("@postalCode", mappedRequest.postalCode);
            parameters.Add("@prefecture", mappedRequest.prefecture);
            parameters.Add("@city", mappedRequest.city);
            parameters.Add("@street", mappedRequest.street);
            parameters.Add("@residenceNumber", mappedRequest.residenceNumber);
            parameters.Add("@userId", userId);
            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<CommonDBResponse>(proc, parameters);
            return dbResponse;
        }
        public async Task<CommonDBResponse> SaveImage(string profileImage, string userId)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_cp_profile_detail";
            parameters.Add("@flag", "ui");
            parameters.Add("@profileImage", profileImage);
            parameters.Add("@userId", userId);
            var dbResponse = await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
            return dbResponse;
        }

        public async Task<List<PhotoGalleryResponseCommon>> GetGalleryPhotoAsync(PhotoGalleryRequestCommon requestModel)
        {
            string proc = "apiproc_cp_photo_galary_list ";
            var parameters = new DynamicParameters();
            parameters.Add("@agentId", requestModel.clubId);
            parameters.Add("@clubCode", requestModel.clubCode);
            parameters.Add("@pageNo", requestModel.pageNo);
            var dbResp = await _dao.ExecuteAsyncList<PhotoGalleryResponseCommon>(proc, parameters);
            return dbResp;

        }

        public async Task<CommonDBResponse> DeleteUserAsync(CommonDBRequest request)
        {
            string proc = "DBO.apiproc_cp_delete_user ";
            var parameters = new DynamicParameters();
            parameters.Add("@agentId", request.agentId);
            parameters.Add("@actionUser", request.actionUserId);
            parameters.Add("@actionIp", request.actionIP);
            parameters.Add("@actionPlatform", request.actionPlatform);
            var dbResp = await _sqlTransactionHandler.ExecuteWithTransactionAsync<CommonDBResponse>(proc, parameters);
            return dbResp;
        }

        public async Task<CommonDBResponse> DeleteUserAsyncWithoutTransaction(CommonDBRequest request)
        {
            string proc = "DBO.apiproc_cp_delete_user ";
            var parameters = new DynamicParameters();
            parameters.Add("@agentId", request.agentId);
            parameters.Add("@actionUser", request.actionUserId);
            parameters.Add("@actionIp", request.actionIP);
            parameters.Add("@actionPlatform", request.actionPlatform);
            var dbResp = await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
            return dbResp;
        }
    }
}
