using System.Net;

namespace hoslog.customer.business.LocationManagement
{
    public interface ILocationManagementBusiness
    {
        Task<(HttpStatusCode, object)> GetLocationList();
    }
}
