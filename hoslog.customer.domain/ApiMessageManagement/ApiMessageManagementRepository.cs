using Dapper;
using hoslog.customer.shared.ApiMessageManagement;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.domain.ApiMessageManagement
{
    public class ApiMessageManagementRepository : IApiMessageManagementRepository
    {
        private readonly RepositoryDao _dao;
        public ApiMessageManagementRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<List<ApiMessageCommon>> GetMessage(ApiMessageRequestCommon request)
        {
            var response = new List<ApiMessageCommon>();
            string proc = "[dbo].[sproc_api_response_message_list]";
            var parameters = new DynamicParameters(
                new
                {
                    request.messageId,
                    request.searchFilter
                }
            );
            response = await _dao.ExecuteAsyncList<ApiMessageCommon>(proc, parameters);
            return response;
        }

        public async Task<ApiMessageCommon> GetMessageViaMessageCode(string messageCode)
        {
            const string proc = "[dbo].[apiproc_get_api_response_message_via_code]";
            var parameters = new DynamicParameters(
                new
                {
                    messageCode
                }
            );
            ApiMessageCommon response = await _dao.ExecuteAsync<ApiMessageCommon>(proc, parameters);
            return response;
        }

    }
}
