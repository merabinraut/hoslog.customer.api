using hoslog.customer.common;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.ApiMessageManagement;
using hoslog.customer.shared;
using hoslog.customer.shared.ApiMessageManagement;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.business.ApiMessageManagement
{
    public class ApiMessageManagementBusiness : IApiMessageManagementBusiness
    {
        public readonly IApiMessageManagementRepository _buss;
        private readonly StringCipher _stringCipher;
        public ApiMessageManagementBusiness(IConfiguration configuration, StringCipher stringCipher)
        {
            _buss = new ApiMessageManagementRepository(configuration);
            _stringCipher = stringCipher;
        }

        public async Task<List<ApiMessageCommon>> GetMessage(ApiMessageRequestCommon request)
        {
            return await _buss.GetMessage(request);
        }

        public async Task<(HttpStatusCode, object)> GetMessageResponse(ApiMessageRequestCommon request)
        {
            var dbRequest = request.MapObject<ApiMessageRequestCommon>();
            dbRequest.messageId = string.IsNullOrEmpty(dbRequest?.messageId) ? null : _stringCipher.Decrypt(dbRequest.messageId);
            dbRequest.searchFilter = string.IsNullOrEmpty(dbRequest?.searchFilter) ? null : dbRequest.searchFilter;

            var dbResponse = await _buss.GetMessage(dbRequest);
            var responseData = dbResponse?
                .MapObjects<ApiMessageCommon>()
                .Select(x =>
                {
                    x.sno = _stringCipher.Encrypt(x.sno);
                    return x;
                }).ToList() ?? new List<ApiMessageCommon>();

            var response = new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = ResponseMessages.Msg200_Success,
                data = responseData
            };

            return (HttpStatusCode.OK, response);
        }

        public async Task<ApiMessageCommon> GetMessageViaMessageCode(string messageCode)
        {
            return await _buss.GetMessageViaMessageCode(messageCode);
        }
    }
}
