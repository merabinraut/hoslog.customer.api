using System.Net;
using hoslog.customer.common;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.PointManagement;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.PointManagement;
using hoslog.customer.shared;
using hoslog.customer.shared.PointManagement;
using Microsoft.AspNetCore.Http;

namespace hoslog.customer.business.PointManagement
{
    public class PointManagementBusiness : IPointManagementBusiness
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly StringCipher _stringCipher;
        private readonly IPointManagementRepository _pointManagementRepository;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        public PointManagementBusiness(IPointManagementRepository pointManagementRepository, IHttpContextAccessor httpContextAccessor, StringCipher stringCipher, ApiResponseMessageService apiResponseMessageService)
        {
            _pointManagementRepository = pointManagementRepository;
            _httpContextAccessor = httpContextAccessor;
            _stringCipher = stringCipher;
            _apiResponseMessageService = apiResponseMessageService;
        }

        public async Task<(HttpStatusCode, object)> GetPoint(int? row = 0, int? fetch = 10, string transactionMode = "")
        {
            var response = new CommonApiResponse
            {
                errors = new List<Errors>()
            };

            var request = new PointManagementRequestCommon
            {
                agentId = _stringCipher.Decrypt(_httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.AgentId).Value),
                transactionMode = string.IsNullOrEmpty(transactionMode) ? null : transactionMode,
                row = row,
                fetch = fetch
            };

            var dbResponse = await _pointManagementRepository.GetPoint(request);
            if (dbResponse?.Count() > 0 && dbResponse?.FirstOrDefault()?.code != "0")
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = ResponseMessages.Msg400_BadRequest;
                response.errors = new List<Errors>
                {
                    new Errors{errorCode=dbResponse?.FirstOrDefault()?.message,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.FirstOrDefault()?.message)  }
                };
                return (HttpStatusCode.BadRequest, response);
            }
            var pagination = new PaginationResponse()
            {
                pageNo = request.row + 1 ?? 1,
                totalRecords = dbResponse?.FirstOrDefault()?.totalData ?? 0,
                recordPerPage = request.fetch ?? 10,
            };
            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success) ?? ResponseMessages.Msg200_Success;
            response.data = new { pagination = pagination, data = dbResponse?.MapObjects<PointManagementResponseModel>() };
            return (HttpStatusCode.OK, response);
        }
    }
}