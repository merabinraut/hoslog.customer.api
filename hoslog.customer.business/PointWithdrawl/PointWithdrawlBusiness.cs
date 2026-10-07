using hoslog.customer.common.Cache;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.PointWithdrawl;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.PointWithdrawl;
using hoslog.customer.shared;
using hoslog.customer.shared.PointWithDrawl;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Reflection;
namespace hoslog.customer.business.PointWithdrawl
{
    public class PointWithdrawlBusiness : IPointWithdrawlBusiness
    {
        private readonly IPointWithdrawlRepository _pointWithdrawlRepository;
        private readonly StringCipher _stringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const decimal MinRedeemAmount = 5000;
        private const decimal RedeemIncrement = 5000;


        public PointWithdrawlBusiness(IPointWithdrawlRepository pointWithdrawlRepository,StringCipher stringCipher,ApiResponseMessageService apiResponseMessageService,IHttpContextAccessor httpContextAccessor)
        {
            _pointWithdrawlRepository = pointWithdrawlRepository;
            _apiResponseMessageService = apiResponseMessageService;
            _stringCipher = stringCipher;
            _httpContextAccessor = httpContextAccessor;
                
        }
        public async Task<(HttpStatusCode, object)> RedeemPointGiftCard(RedeemPointGiftCardModel model)
        {
            var response = new CommonApiResponse
            {
                errors = new List<Errors>()
            };

            string agentId = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == UserClaimsName.AgentId)?.Value;

            if (string.IsNullOrEmpty(agentId))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);

                return (HttpStatusCode.BadRequest, response);
            }

            agentId = _stringCipher.Decrypt(agentId);
            long customerId;

            if (!long.TryParse(agentId, out customerId))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);

                return (HttpStatusCode.BadRequest, response);
            }
            if (model == null || model.pointAmount < MinRedeemAmount)
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = ResponseMessages.Msg400_BadRequest;
                response.data = Array.Empty<string>();
                response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CodeCUE100_Invalid_request,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE100_Invalid_request)
                    }
                };
                return (HttpStatusCode.BadRequest, response);
            }
            if (model.pointAmount % RedeemIncrement != 0)
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = ResponseMessages.Msg400_BadRequest;
                response.data = Array.Empty<string>();
                response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CodeCUE100_Invalid_request,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE100_Invalid_request)
                    }
                };
                return (HttpStatusCode.BadRequest, response);
            }
            var actionIP = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var dbRequest = new PointExchangeCommons
            {
                actionUserId = agentId,
                actionIP = actionIP,
                pointAmount = model.pointAmount.ToString()
            };

            var repoResp = await _pointWithdrawlRepository.RedeemPointGiftCard(dbRequest,customerId);
            if(repoResp == null)
            {
                return (HttpStatusCode.BadRequest, new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest)
                });
            }
            if (repoResp.code != "0")
            {
                return (HttpStatusCode.BadRequest, new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message ?? ResponseCodes.Code400_BadRequest.ToString())
                });
            }

            return (HttpStatusCode.OK, new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message ?? ResponseCodes.Code200_Success)
            });
        }
    }
}
