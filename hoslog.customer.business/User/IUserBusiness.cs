using System.Net;
using hoslog.customer.shared.User;

namespace hoslog.customer.business.User
{
    public interface IUserBusiness
    {
        Task<(HttpStatusCode, object)> GetUserDetailViaUsername(string userName);
        Task<UserDetailCommon> GetCustomerDetail(UserDetailRequestCommon request);
    }
}