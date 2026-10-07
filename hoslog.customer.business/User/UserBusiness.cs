using System.Net;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.User;
using hoslog.customer.shared;
using hoslog.customer.shared.User;

namespace hoslog.customer.business.User
{
    public class UserBusiness : IUserBusiness
    {
        private readonly IUserRepository _userRepository;
        private readonly CommonResponseHandler _commonResponseHandler;
        public UserBusiness(IUserRepository userRepository, CommonResponseHandler commonResponseHandler)
        {
            _userRepository = userRepository;
            _commonResponseHandler = commonResponseHandler;
        }
        public async Task<(HttpStatusCode, object)> GetUserDetailViaUsername(string userName)
        {
            var response = new CommonApiResponse();
            var dbResponse = await _userRepository.GetUserDetailViaUsername(userName);
            if (dbResponse == null || string.IsNullOrEmpty(dbResponse.AgentId))
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>();
            response.data = dbResponse;
            response.code = ResponseCodes.Code200_Success;
            response.message = "User detail fetched successfully";
            return (HttpStatusCode.OK, response);
        }

        public async Task<UserDetailCommon> GetCustomerDetail(UserDetailRequestCommon request)
        {
            return await _userRepository.GetCustomerDetail(request);
        }
    }
}