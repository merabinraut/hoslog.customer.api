using hoslog.customer.common;
using hoslog.customer.common.Models.Plan;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Plans;
using hoslog.customer.shared;
using hoslog.customer.shared.Plans;
using System.Net;

namespace hoslog.customer.business.Plans
{
    public class PlanBusiness : IPlanBusiness
    {
        private readonly IPlanRepository _planRepository;
        private readonly CommonResponseHandler _commonResponseHandler;
        private readonly StringCipher _stringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        public PlanBusiness(IPlanRepository planRepository, StringCipher stringCipher, CommonResponseHandler commonResponseHandler, ApiResponseMessageService apiResponseMessageService)
        {
            _planRepository = planRepository;
            _commonResponseHandler = commonResponseHandler;
            _stringCipher = stringCipher;
            _apiResponseMessageService = apiResponseMessageService;
        }
        public async Task<(HttpStatusCode, object)> GetAdminPlanList()
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };

            var repoResp = await _planRepository.GetAdminPlanList();
            if (repoResp == null || repoResp.Count <= 0)
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
                resp.data = Array.Empty<string>();
                return (HttpStatusCode.OK, resp);
            }

            var planList = repoResp.Select(plans => new PlanCommonResponse
            {
                id = _stringCipher.Encrypt(plans.id),
                planName = plans.planName,
                planType = plans.planType,
                remarks = plans.remarks,
                isActive = (plans.isActive?.ToUpper() == "A") ? true : false,
                strikePrice = !string.IsNullOrEmpty(plans.strikePrice) ? int.Parse(plans.strikePrice) : 0,
                price = !string.IsNullOrEmpty(plans.price) ? int.Parse(plans.price) : 0,
                isStrikeOut = (plans.isStrikeOut?.ToUpper() == "A") ? true : false,
                isPopularity = (plans.isPopularity?.ToUpper() == "Y") ? true : false,
                planTime = plans.planTime,
                liquor = plans.liquor,
                nomination = !string.IsNullOrEmpty(plans.nomination) ? int.Parse(plans.nomination) : 0,
                hasMostPopularTag = plans.hasMostPopularTag,
                planNameEnglish = plans.planNameEnglish
            }).ToList();

            resp.data = planList;
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUSPL100_Plan_details_fetched_successfully);
            return (HttpStatusCode.OK, resp);
        }

        public async Task<(HttpStatusCode, object)> GetClubPlanList(string clubId = "")
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            if (!string.IsNullOrEmpty(clubId))
                clubId = _stringCipher.Decrypt(clubId);
            var repoResp = await _planRepository.GetClubPlanList(clubId);
            if (repoResp.Count() == null && repoResp.Count() == 0)
            {
                return (HttpStatusCode.NotFound, new CommonApiResponse()
                {
                    code = ResponseCodes.Code200_Success,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound)
                });
            }
            var respMapped = repoResp.Select(plans => new ClubPlanResponseModel
            {
                planId = _stringCipher.Encrypt(plans.planId.ToString()),
                planName = plans.planName,
                planPrice = plans.planPrice,
                planTime = plans.planTime,
                noOfNomination = plans.noOfNomination,
                noOfPeople = plans.noOfPeople,
            }).ToList();
            return (HttpStatusCode.OK, new CommonApiResponse()
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
                data = respMapped
            });
        }

        public async Task<(HttpStatusCode, CommonApiResponse)> GetClubAllPlanList(string clubId = "", string clubCode = "", int pageNo = 1)
        {
            if (pageNo <= 0) pageNo = 1;

            string decryptedClubId = _stringCipher.Decrypt(clubId);

            if (!string.IsNullOrEmpty(clubId) && string.IsNullOrEmpty(decryptedClubId))
            {
                return (HttpStatusCode.BadRequest, new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE101_InvalidClub)
                });
            }

            var dbRequest = new ClubPlanListRequestCommon
            {
                clubId = decryptedClubId,
                pageNo = pageNo,
                clubCode = clubCode
            };

            var dbResponse = await _planRepository.GetClubAllPlanList(dbRequest);
            if (dbResponse == null || !dbResponse.Any())
            {
                return (HttpStatusCode.NotFound, new CommonApiResponse()
                {
                    code = ResponseCodes.Code200_Success,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound)
                });
            }

            var response = new ClubAllPlanListResponseModel
            {
                pagination = dbResponse?.FirstOrDefault()?.MapObject<PaginationResponse>() ?? new PaginationResponse(),
                plans = dbResponse?.MapObjects<ClubAllPlanListModel>() ?? new List<ClubAllPlanListModel>()
            };
            response.plans.ForEach(x =>
            {
                x.planId = _stringCipher.Encrypt(x?.planId);
            });

            return (HttpStatusCode.OK, new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
                data = response
            });
        }
    }
}
