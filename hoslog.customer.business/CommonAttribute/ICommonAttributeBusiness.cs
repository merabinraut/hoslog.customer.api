using System.Net;
using hoslog.customer.shared;

namespace hoslog.customer.business.CommonAttribute
{
    public interface ICommonAttributeBusiness
    {
        Task<(HttpStatusCode, object)> GetCommonAttributeList(string flag);
        Task ClearAllCacheAsync();
        #region refer
        Task<(HttpStatusCode, CommonApiResponse)> UpdateReferralClickCount(string referCode);
        #endregion
    }
}
