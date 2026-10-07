using hoslog.customer.shared.ReferralManagement;

namespace hoslog.customer.domain.ReferralManagement
{
    public interface IReferralPointRepository
    {
        Task<List<ReferralPointInfoCommon>> GetReferralPointInfo();
        Task<ReferralPointTotalCommon> GetTotalReferralPoint(string customerId);
        Task<ReferralInfoStatusCommon> GetReferralInfoStatus(string customerId);

    }
}
