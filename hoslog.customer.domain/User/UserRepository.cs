using Dapper;
using hoslog.customer.shared.User;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.User
{
    public class UserRepository : IUserRepository
    {
        private readonly RepositoryDao _dao;
        public UserRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<UserDetailCommon> GetUserDetailViaUsername(string userName)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_cp_get_customer_detail_via_username";
            parameters.Add("@UserName", userName);
            var dbResponse = await _dao.ExecuteAsync<UserDetailCommon>(proc, parameters);
            return dbResponse;
        }

        public async Task<UserDetailCommon> GetCustomerDetail(UserDetailRequestCommon request)
        {
            var parameters = new DynamicParameters(new
            {
                request.customerId,
                request.mobileNumber,
                request.emailAddress,
                request.processId,
                request.processType
            });
            string proc = "apiproc_cp_get_customer_detail";
            var response = await _dao.ExecuteAsync<UserDetailCommon>(proc, parameters);
            return response;
        }
    }
}
