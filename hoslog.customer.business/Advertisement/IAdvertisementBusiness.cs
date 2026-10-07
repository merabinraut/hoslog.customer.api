using System.Net;

namespace hoslog.customer.business.Advertisement
{
    public interface IAdvertisementBusiness
    {
        Task<(HttpStatusCode, object)> GetAdvertisementList();
    }
}
