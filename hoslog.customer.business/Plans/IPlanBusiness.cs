using System.Net;
using hoslog.customer.shared;

namespace hoslog.customer.business.Plans
{
  public interface IPlanBusiness
  {
    Task<(HttpStatusCode, object)> GetAdminPlanList();
    Task<(HttpStatusCode, object)> GetClubPlanList(string clubId = "");
    Task<(HttpStatusCode, CommonApiResponse)> GetClubAllPlanList(string clubId = "", string clubCode = "", int pageNo = 1);
  }
}
