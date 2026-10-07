using hoslog.customer.shared.SSE;

namespace hoslog.customer.domain.SSE;

public interface IVisitNowRepository
{
    Task<List<VisitNowStatusChangedRow>> RecalculateStatusAsync(
        int locationId,
        string timeZoneOffsetValue
    );
}