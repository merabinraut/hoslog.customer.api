using hoslog.customer.shared.ApiMessageManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.domain.ApiMessageManagement
{
    public interface IApiMessageManagementRepository
    {
        Task<List<ApiMessageCommon>> GetMessage(ApiMessageRequestCommon request);
        Task<ApiMessageCommon> GetMessageViaMessageCode(string messageCode);
    }
}
