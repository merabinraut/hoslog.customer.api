using System.Net;

namespace hoslog.customer.business.Banner
{
    public interface IBannerBusiness
    {
        Task<(HttpStatusCode, object)> GetBannerList();
    }
}
