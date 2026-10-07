using System.Net;
using hoslog.customer.business.CommonHandler;
using hoslog.customer.common;
using hoslog.customer.common.Models;
using hoslog.customer.common.Models.Multitenant;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Multitenant;
using hoslog.customer.shared;
using hoslog.customer.shared.Multitenant;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.business.Multitenant
{
    public class ClubInformationMultitenantBusiness : IClubInformationMultitenantBusiness
    {
        private readonly IClubInformationMultitenantRepository _clubInformationRepository;
        private readonly StringCipher _stringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly IConfiguration _configuration;
        private readonly AmazonSettingsCommon _amazonSettingsCommon;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string[] _baseDomains;
        private readonly string _requestSourceUrl;
        private readonly CommonAttributeStringCipher _commonAttributeStringCipher;
        private readonly CommonResponseHandler _commonResponseHandler;
        public ClubInformationMultitenantBusiness(IClubInformationMultitenantRepository clubInformationRepository, StringCipher stringCipher,
            ApiResponseMessageService apiResponseMessageService, IConfiguration configuration, CommonAttributeStringCipher commonAttributeStringCipher,
            IHttpContextAccessor httpContextAccessor, CommonResponseHandler commonResponseHandler)
        {
            _clubInformationRepository = clubInformationRepository;
            _stringCipher = stringCipher;
            _apiResponseMessageService = apiResponseMessageService;
            _configuration = configuration;
            _amazonSettingsCommon = new AmazonSettingsCommon
            {
                BucketName = _configuration["BucketName"] ?? string.Empty,
                BaseUrl = _configuration["BaseUrl"] ?? string.Empty,
                ConfigAccess = _configuration["ConfigAccess"] ?? string.Empty,
                ConfigSecret = _configuration["ConfigSecret"] ?? string.Empty,
                ImageVirtualPath = _configuration["ImageVirtualPath"] ?? string.Empty,
                PreviousImageUrl = _configuration["PreviousImageUrl"] ?? string.Empty
            };
            _commonAttributeStringCipher = commonAttributeStringCipher;
            _httpContextAccessor = httpContextAccessor;
            _baseDomains = _configuration.GetSection("MultitenantBaseDomain").Get<string[]>();
            _requestSourceUrl = GetRequestSourceUrl();
            _commonResponseHandler = commonResponseHandler;
        }

        private string GetRequestSourceUrl()
        {
            var headerValue = _httpContextAccessor?.HttpContext?.Items["RequestSourceUrl"] as string;

            if (string.IsNullOrEmpty(headerValue))
            {
                var originHeader = _httpContextAccessor?.HttpContext?.Request.Headers["Origin"].FirstOrDefault();
                var refererHeader = _httpContextAccessor?.HttpContext?.Request.Headers["Referer"].FirstOrDefault();
                headerValue = originHeader ?? refererHeader;
            }
            return headerValue ?? string.Empty;
        }

        public async Task<(HttpStatusCode, CommonApiResponse)> GetClubBasicInfomration(string clubCode = "", string clubId = "")
        {
            var response = new CommonApiResponse
            {
                errors = new List<Errors>()
            };

            string clubAgentId = string.IsNullOrEmpty(clubId) ? string.Empty : _stringCipher.Decrypt(clubId);
            var subDomainName = Helper.GetSubdomain(_requestSourceUrl, _baseDomains);

            if (string.IsNullOrEmpty(subDomainName) && string.IsNullOrEmpty(clubCode) && string.IsNullOrEmpty(clubAgentId))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = ResponseMessages.Msg400_BadRequest;
                response.data = Array.Empty<string>();
                response.errors = new List<Errors>
                {
                    new Errors{errorCode=ResponseCodes.CodeCUE100_Invalid_request,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE100_Invalid_request)  }
                };
                return (HttpStatusCode.BadRequest, response);
            }

            var responseModel = new ClubDetailCommonMultitenantModel();
            var timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
            var dbResponse = await _clubInformationRepository.GetClubBasicInfomration(subDomainName, clubCode, clubAgentId, timeZoneOffsetValue);

            if (!string.IsNullOrEmpty(dbResponse.clubId))
            {
                var clubPlanDetailsResponse = await _clubInformationRepository.GetClubPlanDetails(dbResponse.clubId);
                var clubPlanDetails = clubPlanDetailsResponse.MapObjects<ClubPlanDetailMultitenantModel>();

                var clubSpecificPlanResponse = await _clubInformationRepository.GetClubOwnPlanDetails(dbResponse.clubId);
                var clubSpecificPlanDetails = clubSpecificPlanResponse.MapObjects<ClubSpecificPlanDetailModel>();
                var clubPlanGroupedByName = clubSpecificPlanDetails?.GroupBy(clubPlan => clubPlan.planName).Select(group => new { ClubPlanName = group.Key, GetClubPlanGroupDetail = group.ToList() }).ToList();

                var planGroupedByName = clubPlanDetails?.GroupBy(plan => plan.planName).Select(group => new { PlanName = group.Key, GetPlanGroupDetail = group.ToList() }).ToList();
                var validLabels = new List<string> { "最大予約人数", "ラストオーダー時間", "最終入店時間" };
                var filteredPlans = planGroupedByName?
                    .Select(plan => new
                    {
                        planName = plan.PlanName,
                        details = plan.GetPlanGroupDetail
                                    .Where(detail => validLabels.Contains(detail.label))
                                    .Select(detail => new
                                    {
                                        Label = detail.label,
                                        Value = detail.labelValue
                                    })
                    });

                var filteredclubPlanPlans = clubPlanGroupedByName?
                   .Select(clubPlan => new
                   {
                       planName = clubPlan.ClubPlanName,
                       details = clubPlan.GetClubPlanGroupDetail
                                   .Where(detail => validLabels.Contains(detail.label))
                                   .Select(detail => new
                                   {
                                       Label = detail.label,
                                       Value = detail.labelValue
                                   })
                   });

                var clubBasicInformationModels = dbResponse?.MapObject<ClubBasicInformationMultitenantResponseModel>();
                clubBasicInformationModels.clubId = _stringCipher.Encrypt(clubBasicInformationModels?.clubId);
                clubBasicInformationModels.lastOrderTime = string.IsNullOrEmpty(clubBasicInformationModels.lastOrderTime) ? null : clubBasicInformationModels.lastOrderTime;
                clubBasicInformationModels.locationId = string.IsNullOrEmpty(clubBasicInformationModels.locationId) ? string.Empty : _stringCipher.Encrypt(clubBasicInformationModels.locationId);
                clubBasicInformationModels.tag2 = string.IsNullOrEmpty(clubBasicInformationModels.tag2) ? string.Empty : "ランク" + clubBasicInformationModels.tag2;
                clubBasicInformationModels.hostCount = !string.IsNullOrEmpty(clubBasicInformationModels.hostCount?.ToString()) ? Convert.ToInt32(clubBasicInformationModels.hostCount?.ToString()) : 0;
                clubBasicInformationModels.galleryImageCount = !string.IsNullOrEmpty(clubBasicInformationModels.galleryImageCount?.ToString()) ? Convert.ToInt32(clubBasicInformationModels.galleryImageCount?.ToString()) : 0;
                clubBasicInformationModels.reviewCount = !string.IsNullOrEmpty(clubBasicInformationModels.reviewCount?.ToString()) ? Convert.ToInt32(clubBasicInformationModels.reviewCount?.ToString()) : 0;
                clubBasicInformationModels.location = new hoslog.customer.shared.LocationResponse
                {
                    url = dbResponse?.locationURL,
                    longitude = dbResponse?.longitude,
                    latitude = dbResponse?.latitude
                };
                clubBasicInformationModels.resClubLogo = new ImageResponse
                {
                    alt = clubBasicInformationModels.clubNameEng,
                    description = "club logo",
                    path = ImageHandler.ProcessedImage(_amazonSettingsCommon, clubBasicInformationModels.clubLogo),
                };
                clubBasicInformationModels.resClubCoverPhoto = new ImageResponse
                {
                    alt = string.IsNullOrWhiteSpace(clubBasicInformationModels.clubCoverPhoto) ? null : clubBasicInformationModels.clubNameEng,
                    description = string.IsNullOrWhiteSpace(clubBasicInformationModels.clubCoverPhoto) ? null : "club cover image",
                    path = string.IsNullOrWhiteSpace(clubBasicInformationModels.clubCoverPhoto) ? null : ImageHandler.ProcessedImage(_amazonSettingsCommon, clubBasicInformationModels.clubCoverPhoto),
                };
                clubBasicInformationModels.otherHoliday = !string.IsNullOrEmpty(clubBasicInformationModels?.otherHoliday)
                             ? string.Join(",", clubBasicInformationModels.otherHoliday.Split(',')
                                 .Select(x => _commonAttributeStringCipher.Encrypt(x)))
                             : string.Empty;

                response.code = ResponseCodes.Code200_Success;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
                response.data = new
                {
                    club_basic_info = clubBasicInformationModels,
                    plan_detail = filteredPlans,
                    club_plan_detail = filteredclubPlanPlans
                };
                return (HttpStatusCode.OK, response);
            }
            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
            response.data = Array.Empty<string>();
            return (HttpStatusCode.OK, response);
        }

        public async Task<(HttpStatusCode, CommonApiResponse)> GetClubReviewAndRatings(string clubId = "", string clubCode = "", int pageNo = 0)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            var subDomainName = Helper.GetSubdomain(_requestSourceUrl, _baseDomains);
            string clubAgentId = string.IsNullOrEmpty(clubId) ? string.Empty : _stringCipher.Decrypt(clubId);
            var dbResponse = await _clubInformationRepository.GetClubReviewAndRatings(subDomainName, clubAgentId, clubCode, pageNo);
            var userReviewInfo = dbResponse.MapObjects<ClubReviewsMultitenantModel>();
            if (dbResponse.Count() <= 0 || dbResponse == null)
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
                resp.data = Array.Empty<string>();
                return (HttpStatusCode.OK, resp);
            }
            var pagination = new PaginationResponse()
            {
                pageNo = dbResponse?.FirstOrDefault()?.pageNo,
                totalRecords = dbResponse?.FirstOrDefault()?.totalRecords,
                recordPerPage = dbResponse?.FirstOrDefault()?.recordPerPage,
            };
            userReviewInfo = dbResponse.Select(item => new ClubReviewsMultitenantModel
            {
                clubId = _stringCipher.Encrypt(item.clubId),
                reviewId = _stringCipher.Encrypt(item.reviewId),
                visitedDate = item.visitedDate,
                commentedOn = item.commentedOn,
                customerName = item.customerName,
                ratingScale = item.ratingScale,
                isCourseExtended = item.isCourseExtended,
                didMetFavourite = item.didMetFavourite,
                customerImageResponse = new ImageResponse
                {
                    alt = item.customerName,
                    description = "customer image",
                    path = ImageHandler.ProcessedImage(_amazonSettingsCommon, item.customerImage)
                },
                getClubReviewHostList = item.getClubReviewHostList.Select(host => new ClubReviewHostMultitenantModel
                {
                    hostId = _stringCipher.Encrypt(host.hostId),
                    hostCode = host.hostCode,
                    hostImageResponse = new ImageResponse
                    {
                        alt = host.hostName,
                        description = "host image",
                        path = ImageHandler.ProcessedImage(_amazonSettingsCommon, host.hostImage)
                    },
                    hostName = host.hostName,
                    isMVPHost = host.isMVPHost
                }).ToList(),
                getClubReviewRemarkList = item.getClubReviewRemarkList.Select(remark => new ClubReviewRemarkMultitenantModel
                {
                    englishRemark = remark.englishRemark,
                    japaneseRemark = remark.japaneseRemark,
                    remarkType = remark.remarkType
                }).ToList(),
                getIntentRemarks = item.getIntentRemarks.Select(intent => new IntentRemarksMultitenantModel
                {
                    intentEnglishRemark = intent.intentEnglishRemark,
                    intentJapaneseRemark = intent.intentJapaneseRemark,
                    intentRemarkType = intent.intentRemarkType
                }).ToList(),
                qnaRemarks = item.qnaRemarks.Select(qnaRemarks => new QNARemarksMultitenantModel
                {
                    qnaEngQuestion = qnaRemarks.qnaEngQuestion,
                    qnaJpQuestion = qnaRemarks.qnaJpQuestion,
                    qnaJpShortQuestion = qnaRemarks.qnaJpShortQuestion,
                    qnaEngAnswer = qnaRemarks.qnaEngAnswer,
                    qnaJpAnswer = qnaRemarks.qnaJpAnswer
                }).ToList(),
            }).ToList();
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);
            resp.data = new
            {
                pagination = pagination,
                user_review_rating_info = userReviewInfo
            };
            return (HttpStatusCode.OK, resp);
        }

        public async Task<(HttpStatusCode, CommonApiResponse)> GetClubWeeklySchedule(string agentId = "", string clubCode = "", int duration = 1)
        {
            var response = new CommonApiResponse();
            var subDomainName = Helper.GetSubdomain(_requestSourceUrl, _baseDomains);
            agentId = !string.IsNullOrEmpty(agentId) ? _stringCipher.Decrypt(agentId) : string.Empty;
            if (string.IsNullOrEmpty(subDomainName) && string.IsNullOrEmpty(agentId) && string.IsNullOrEmpty(clubCode))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = ResponseMessages.Msg400_BadRequest;
                response.errors = new List<Errors>
                {
                    new Errors{errorCode=ResponseCodes.CodeCUE101_Invalid_club,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE101_Invalid_club)   }
                };
                return (HttpStatusCode.BadRequest, response);
            }

            var clubScheduleResponse = await _clubInformationRepository.GetClubWeeklySchedule(subDomainName, agentId, clubCode, duration);
            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);
            response.data = clubScheduleResponse.MapObjects<ClubScheduleMultitenantModel>();
            return (HttpStatusCode.OK, response);
        }

        public async Task<(HttpStatusCode, CommonApiResponse)> GetScheduleListAsync(string clubId, string clubCode, long? date)
        {
            var resp = new CommonApiResponse();
            var requesMapped = new ScheduleDetailsRequestMultitenantCommon()
            {
                clubId = clubId,
                clubCode = clubCode,
                date = date,
                subDomainName = Helper.GetSubdomain(_requestSourceUrl, _baseDomains)
            };
            requesMapped.clubId = string.IsNullOrEmpty(requesMapped.clubId) ? string.Empty : _stringCipher.Decrypt(requesMapped.clubId);

            if (string.IsNullOrEmpty(requesMapped.subDomainName) && string.IsNullOrEmpty(requesMapped.clubId) && string.IsNullOrEmpty(requesMapped.clubCode))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = ResponseMessages.Msg400_BadRequest;
                resp.data = Array.Empty<string>();
                resp.errors = new List<Errors>
                {
                    new Errors{errorCode=ResponseCodes.CodeCUE101_Invalid_club,errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE101_Invalid_club)  }
                };
                return (HttpStatusCode.BadRequest, resp);
            }

            requesMapped.timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
            var response = await _clubInformationRepository.GetScheduleListAsync(requesMapped);
            if (response.Count > 0 && response.FirstOrDefault().code.ToString().ToUpper() == "0")
            {
                var respMapped = response.MapObjects<ScheduleResponseMultitenantModel>();
                respMapped = response.Select(item => new ScheduleResponseMultitenantModel
                {
                    ScheduleId = _stringCipher.Encrypt(item.ScheduleId),
                    ScheduleTitle = item.ScheduleTitle,
                    ScheduleDate = item.ScheduleDate,
                    ScheduleDescription = item.ScheduleDescription,
                    EventType = _stringCipher.Encrypt(item.EventType),
                    ClosingDate = item.closingDate,
                    image = !string.IsNullOrEmpty(item.ScheduleImage) ? new ImageResponse()
                    {
                        alt = item.ScheduleTitle,
                        path = ImageHandler.ProcessedImage(_amazonSettingsCommon, item.ScheduleImage),
                        description = null
                    } : new ImageResponse()
                }).ToList();

                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
                resp.data = respMapped;
                return (HttpStatusCode.OK, resp);
            }
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
            resp.data = Array.Empty<string>();
            return (HttpStatusCode.OK, resp);
        }

        public async Task<(HttpStatusCode, CommonApiResponse)> GetClubNoticeAsync(int pageNo, string clubId = "", string clubCode = "")
        {
            var resp = new CommonApiResponse();
            NoticeDetailRequestMultitenantCommon request = new NoticeDetailRequestMultitenantCommon
            {
                subDomainName = Helper.GetSubdomain(_requestSourceUrl, _baseDomains),
                clubId = clubId,
                pageNo = pageNo,
                clubCode = clubCode
            };

            request.clubId = string.IsNullOrEmpty(request.clubId) ? string.Empty : _stringCipher.Decrypt(request.clubId);
            if (string.IsNullOrEmpty(request.subDomainName) && string.IsNullOrEmpty(request.clubId) && string.IsNullOrEmpty(request.clubCode))
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>(responseDetailMessage: "One or more field validations failed",
                fieldErrors: new() { new() { errorCode = ResponseCodes.CodeCUE104_ClubId_or_Clubcode_is_required, errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE104_ClubId_or_Clubcode_is_required) } });

            var response = await _clubInformationRepository.GetClubNoticeAsync(request);
            if (response?.Count > 0 && response?.FirstOrDefault()?.code == "0")
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
                resp.data = response.MapObjects<NoticeDetailResponseMultitenantModel>();
                return (HttpStatusCode.OK, resp);
            }
            else if (response?.Count > 0 && response?.FirstOrDefault()?.code == "1")
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = ResponseMessages.Msg400_BadRequest;
                resp.data = Array.Empty<string>();
                resp.errors = new List<Errors>
                {
                    new Errors{errorCode=resp.message,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(resp.message)   }
                };
                return (HttpStatusCode.BadRequest, resp);
            }
            else
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
                resp.data = Array.Empty<string>();
            }
            return (HttpStatusCode.BadRequest, resp);
        }

        public async Task<(HttpStatusCode, CommonApiResponse)> GetGalleryPhotoAsync(string clubId = "", string clubCode = "")
        {
            var resp = new CommonApiResponse();
            var requestModel = new PhotoGalleryRequestMultitenantCommon
            {
                clubId = clubId,
                clubCode = clubCode,
                subDomainName = Helper.GetSubdomain(_requestSourceUrl, _baseDomains)
            };
            requestModel.clubId = string.IsNullOrEmpty(requestModel.clubId) ? string.Empty : _stringCipher.Decrypt(requestModel.clubId);

            if (string.IsNullOrEmpty(requestModel.subDomainName) && string.IsNullOrEmpty(requestModel.clubId) && string.IsNullOrEmpty(requestModel.clubCode))
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>(responseDetailMessage: "One or more field validations failed",
                 fieldErrors: new() { new() { errorCode = ResponseCodes.CodeCUE104_ClubId_or_Clubcode_is_required, errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE104_ClubId_or_Clubcode_is_required) } });
            }
            var recordnotfound = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
            var recordfound = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
            var response = await _clubInformationRepository.GetGalleryPhotoAsync(requestModel);

            if (response.Count() > 0 && response?.FirstOrDefault()?.code == "0")
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = recordfound;
                resp.data = new PhotoGalleryResponseModel
                {
                    pagination = response?.FirstOrDefault()?.MapObject<PaginationResponse>() ?? new PaginationResponse(),
                    images = response.Select(item => new ImageResponse
                    {
                        alt = item.imageTitle,
                        path = ImageHandler.ProcessedImage(_amazonSettingsCommon, item?.imagePath),
                        description = item.imageTitle
                    }).ToList()
                };
                return (HttpStatusCode.OK, resp);
            }
            else if (response.Count() > 0 && response?.FirstOrDefault()?.code == "1")
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>(await _apiResponseMessageService.GetMessageByCodeAsync(response[0].message));
            }
            else
            {
                resp.code = ResponseCodes.Code200_Success;
                if (response?.Count() == 0)
                {
                    resp.message = recordnotfound;
                    return (HttpStatusCode.OK, resp);
                }
                resp.message = !string.IsNullOrEmpty(response[0].message) ? await _apiResponseMessageService.GetMessageByCodeAsync(response[0].message) : recordnotfound;
                resp.data = Array.Empty<string>();
            }
            return (HttpStatusCode.BadRequest, resp);
        }

        #region Subdomain validation
        public async Task<CommonDBResponse> ValidateClubSubDomain(string subDomainName, string clubCode)
        {
            return await _clubInformationRepository.ValidateClubSubDomain(subDomainName, clubCode);
        }
        #endregion
    }
}