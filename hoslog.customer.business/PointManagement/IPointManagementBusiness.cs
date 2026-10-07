using System.Net;

namespace hoslog.customer.business.PointManagement
{
    public interface IPointManagementBusiness
    {
        Task<(HttpStatusCode, object)> GetPoint(int? row = 0, int? fetch = 10, string transactionMode = "");
    }
}