using hoslog.customer.shared.ApiMessageManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.business.ApiMessageManagement
{
    public interface IApiMessageManagementBusiness
    {
        Task<(HttpStatusCode, object)> GetMessageResponse(ApiMessageRequestCommon request);
        Task<List<ApiMessageCommon>> GetMessage(ApiMessageRequestCommon request);
        Task<ApiMessageCommon> GetMessageViaMessageCode(string messageCode);
    }
}
