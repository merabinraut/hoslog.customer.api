using hoslog.customer.shared.User;

namespace hoslog.customer.domain.User
{
    public interface IUserRepository
    {
        Task<UserDetailCommon> GetUserDetailViaUsername(string userName);
        Task<UserDetailCommon> GetCustomerDetail(UserDetailRequestCommon request);
    }
}
