using hoslog.customer.shared;
using hoslog.customer.shared.Common;

namespace hoslog.customer.domain.CommonAttribute
{
    public interface ICommonAttributeRepository
    {
        Task<List<AttributeCommon>> GetCommonAttributeList(string flag);
        Task<CommonDBResponse> UpdateReferralClickCount(ReferralClickCountUpdateRequestCommon request);
    }
}
