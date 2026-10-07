using hoslog.customer.shared.PointManagement;

namespace hoslog.customer.domain.PointManagement
{
    public interface IPointManagementRepository
    {
        Task<List<PointManagementResponseCommon>> GetPoint(PointManagementRequestCommon request);
    }
}