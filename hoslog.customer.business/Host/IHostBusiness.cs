using System.Net;

namespace hoslog.customer.business.Host
{
    public interface IHostBusiness
    {
        Task<(HttpStatusCode, object)> GetHostDetailsAsync(string hostId, string hostCode);
        Task<(HttpStatusCode, object)> GetHostListAsync(string clubId, string clubCode);
        Task<(HttpStatusCode, object)> GetScheduleListAsync(string clubId, string clubCode, long? date);
        Task<(HttpStatusCode, object)> GetHostsWithRankOrderingAsync(string clubId = null, string clubCode = null, int pageNo = 1, int pageSize = 12);

        Task<(HttpStatusCode, object)> GetHostDetailsV2Async(string hostId, long hostCode);
    }
}
