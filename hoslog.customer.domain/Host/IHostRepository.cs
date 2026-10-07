using hoslog.customer.common.Host;
using hoslog.customer.shared.Host;

namespace hoslog.customer.domain.Host
{
    public interface IHostRepository
    {
        Task<(GetHostListDetailsCommon, List<Skills>, List<Personality>, List<HostGallaryImage>)> GetHostDetailsAsync(string hostId, string hostCode, string userId);
        Task<List<HostDetailResponseCommon>> GetHostListAsync(HostDetailRequestCommon requestCommon);
        Task<IList<HostDetailResponseCommon>> GetRankedHostListAsync(HostDetailRequestCommon requestCommon);
        Task<List<ScheduleResponseCommon>> GetScheduleListAsync(ScheduleDetailsRequestCommon requesMapped);
        Task<List<HostListResponseCommon>> HostsWithRankOrdering(HostListRequestCommon request);
        Task<(HostDetailsV2Common? hostDetails, List<HostImagesV2Common> hostImages)>
            GetHostDetailsV2Async(long? hostId = null, long? hostCode = null, long? userId = null);
    }
}
