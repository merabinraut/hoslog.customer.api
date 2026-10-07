using hoslog.customer.common;
using hoslog.customer.common.Models;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.Middleware.AmazonCognito;
using hoslog.customer.common.Models.Reservation;
using hoslog.customer.common.Models.ReservationModel;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Reservation;
using hoslog.customer.shared;
using hoslog.customer.shared.Reservation;
using hoslog.customer.shared.ReservationModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using commonhandler = hoslog.customer.business.CommonHandler;
using Help = hoslog.customer.common.Helper;


namespace hoslog.customer.business.Reservation
{
    public class ReservationHistoryBusiness : IReservationHistoryBusiness
    {
        private readonly IReservationHistoryRepository _resvRepository;
        private readonly CommonResponseHandler _commonResponseHandler;
        private readonly StringCipher _stringCipher;
        private readonly ValidationHelper _validationHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly IReservationRepository _repos;
        private readonly LineAPIService _lineAPIService;
        private readonly IConfiguration _configuration;
        public ReservationHistoryBusiness(IReservationHistoryRepository resvRepository, StringCipher stringCipher, ValidationHelper validationHelper, IHttpContextAccessor httpContextAccessor, CommonResponseHandler commonResponseHandler, ApiResponseMessageService apiResponseMessageService, IReservationRepository repos, LineAPIService lineAPIService, IConfiguration configuration)
        {
            _resvRepository = resvRepository;
            _commonResponseHandler = commonResponseHandler;
            _stringCipher = stringCipher;
            _validationHelper = validationHelper;
            _httpContextAccessor = httpContextAccessor;
            _apiResponseMessageService = apiResponseMessageService;
            _repos = repos;
            _lineAPIService = lineAPIService;
            _configuration = configuration;
        }
        public async Task<(HttpStatusCode, object)> GetReservationList(PaginationRequestModel objPaginationRequestModel, AmazonSettingsCommon common, string userId, string reservationStatus = "")
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            userId = _stringCipher.Decrypt(userId);
            var repoResp = await _resvRepository.GetReservationList(objPaginationRequestModel, userId, reservationStatus);
            if (repoResp.Count == 0)
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
                resp.data = new ReservationCommonData();
                return (HttpStatusCode.OK, resp);
            }
            else
            {
                var resList = new ReservationCommonData
                {
                    pagination = repoResp.Select(review => new PaginationResponse
                    {
                        pageNo = objPaginationRequestModel.pageNo,
                        totalRecords = repoResp.FirstOrDefault()?.totalRecords,
                        recordPerPage = repoResp.FirstOrDefault()?.recordPerPage,
                    }).FirstOrDefault(),
                    reservationList = repoResp.Select(revs => new ReservationCommonResponse
                    {
                        reservationId = _stringCipher.Encrypt(revs?.reservationId),
                        locationId = _stringCipher.Encrypt(revs?.locationId),
                        clubId = _stringCipher.Encrypt(revs?.clubId),
                        clubCode = revs.clubCode,
                        // reservedDate = Help.ConvertToUnixTimeMilliseconds(Convert.ToDateTime(revs.reservedDate)),
                        // visitDate = Help.ConvertToUnixTimeMilliseconds(Convert.ToDateTime(revs.visitDate)),
                        // visitTime = revs.visitTime,
                        reservedDate = revs.reservedDate,
                        visitDateTime = revs.visitDateTime,
                        invoiceId = revs.invoiceId,
                        clubNameEng = revs.clubNameEng,
                        clubName = revs.clubName,
                        groupName = revs.groupName,
                        mobileNumber = revs.mobileNumber,
                        tag1 = revs.tag1,
                        tag2 = !string.IsNullOrEmpty(revs.tag2) ? "ランク" + "" + revs.tag2 : "",
                        tag3 = revs.tag3,
                        tag4 = revs.tag4,
                        tag5 = revs.tag5,
                        status = revs.status,
                        price = int.Parse(revs.price),
                        noOfPeople = int.Parse(revs.noOfPeople),
                        canChangeTime = Convert.ToBoolean(revs.canChangeTime),
                        canCancel = Convert.ToBoolean(revs.canCancel),
                        canRedo = Convert.ToBoolean(revs.canRedo),
                        canDelete = Convert.ToBoolean(revs.canDelete),
                        image = new ImageResponse
                        {
                            alt = revs.clubName,
                            path = commonhandler.ImageHandler.ProcessedImage(common, revs.clubLogo),
                            description = "club logo"
                        },
                        location = new LocationResponse
                        {
                            url = revs.locationURL,
                            longitude = revs.longitude,
                            latitude = revs.latitude
                        }
                    }).ToList()
                };
                resp.data = resList;
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUSRES101_ReservationListFetchedSuccess);
                return (HttpStatusCode.OK, resp);
            }
        }
        public async Task<(HttpStatusCode, object)> GetReservationDetail(AmazonSettingsCommon common, string userId, ReservationModel objReservationModel)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            var validationResult = await _validationHelper.HandleModelValidationAsync(objReservationModel, req => new ReservationRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;
            userId = _stringCipher.Decrypt(userId);
            objReservationModel.reservationId = _stringCipher.Decrypt(objReservationModel.reservationId);

            var repoReservationResp = await _resvRepository.GetReservationDetail(userId, objReservationModel);
            if (repoReservationResp == null)
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
                resp.data = Array.Empty<string>();
                return (HttpStatusCode.OK, resp);
            }

            ReservationDetailResponse obj = new ReservationDetailResponse();
            obj.reservationId = _stringCipher.Encrypt(repoReservationResp.reservationId);
            obj.customerId = _stringCipher.Encrypt(repoReservationResp.customerId);
            // obj.reservedDate = Help.ConvertToUnixTimeMilliseconds(Convert.ToDateTime(repoReservationResp.reservedDate));
            obj.clubId = _stringCipher.Encrypt(repoReservationResp.clubId);
            obj.clubNameEng = repoReservationResp.clubNameEng;
            obj.clubName = repoReservationResp.clubName;
            obj.clubAddress = repoReservationResp.clubAddress;
            obj.mobileNumber = repoReservationResp.mobileNumber;
            obj.locationName = repoReservationResp.locationName;
            // obj.visitDate = Help.ConvertToUnixTimeMilliseconds(Convert.ToDateTime(repoReservationResp.visitDate));
            // obj.visitTime = repoReservationResp.visitTime;
            obj.reservedDate = repoReservationResp.reservedDate;
            obj.visitDateTime = repoReservationResp.visitDateTime;
            obj.clubCode = repoReservationResp.clubCode;
            obj.planDetail = new PlanDetailResponse
            {
                planName = repoReservationResp.planName,
                price = Convert.ToInt32(repoReservationResp.price),
                planTime = repoReservationResp.planTime,
                noOfPeople = Convert.ToInt32(repoReservationResp.noOfPeople),
                liquor = repoReservationResp.liquor,
                nomination = Convert.ToInt32(repoReservationResp.nomination),
                isPopularity = Convert.ToBoolean(repoReservationResp.isPopularity),
            };

            obj.clubLogo = new ImageResponse
            {
                alt = repoReservationResp.clubName,
                path = commonhandler.ImageHandler.ProcessedImage(common, repoReservationResp.clubLogo),
                description = null

            };
            obj.visitCode = repoReservationResp.visitCode;

            var hostDetailsList = repoReservationResp.hostDetails != null
                ? JsonConvert.DeserializeObject<List<HostDetailCommon>>(repoReservationResp.hostDetails.ToString())
                : new List<HostDetailCommon>();
            obj.hostDetails = hostDetailsList?.Select(prfs => new HostDetailResponse
            {
                hostId = !string.IsNullOrEmpty(prfs.hostId) ? _stringCipher.Encrypt(prfs.hostId) : prfs.hostId,
                image = new ImageResponse
                {
                    alt = prfs.hostName,
                    path = CommonHandler.ImageHandler.ProcessedImage(common, prfs.image),
                    description = null
                },
            }).ToList();
            resp.data = obj;
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUSRES100_ReservationDetailsFetchedSuccess);
            return (HttpStatusCode.OK, resp);
        }
        public async Task<(HttpStatusCode, object)> CancelReservation(ReservationCancelModel objReservationCancelModel, string userId)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            objReservationCancelModel.actionIp = _httpContextAccessor.HttpContext?.GetIpAddress();
            var validationResult = await _validationHelper.HandleModelValidationAsync(objReservationCancelModel, req => new ReservationCancelRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;
            userId = _stringCipher.Decrypt(userId);
            objReservationCancelModel.reservationId = _stringCipher.Decrypt(objReservationCancelModel.reservationId);
            var repoResp = await _resvRepository.CancelReservation(userId, objReservationCancelModel);
            if (repoResp.code == "1")
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = repoResp.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }
            if (!string.IsNullOrEmpty(repoResp.Id))
            {
                LineGroupAfterReservationCommon obj = new LineGroupAfterReservationCommon
                {
                    reservationId = objReservationCancelModel.reservationId,
                    flag = "cancelres",
                    timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty
                };
                var message = await _repos.GetMessageToSendInLine(obj);
                var lineClientSecret = _configuration["LineMessagingClientSecret"] ?? string.Empty;
                var sendMessageToLinegroup = await _lineAPIService.SendMessageToLineGroup(lineClientSecret, repoResp?.Id, message);
            }
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message);
            return (HttpStatusCode.OK, resp);
        }
        public async Task<(HttpStatusCode, object)> DeleteReservation(ReservationCancelModel objReservationCancelModel, string userId)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            objReservationCancelModel.actionIp = _httpContextAccessor.HttpContext?.GetIpAddress();

            var validationResult = await _validationHelper.HandleModelValidationAsync(objReservationCancelModel, req => new ReservationCancelRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            userId = _stringCipher.Decrypt(userId);
            objReservationCancelModel.reservationId = _stringCipher.Decrypt(objReservationCancelModel.reservationId);

            var repoResp = await _resvRepository.DeleteReservation(userId, objReservationCancelModel);
            if (repoResp.code == "1")
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = repoResp.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message);
            return (HttpStatusCode.OK, resp);
        }

        public async Task<(HttpStatusCode, object)> GetReservationClubTime(string userId, ReservationModel objReservationModel)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            var validationResult = await _validationHelper.HandleModelValidationAsync(objReservationModel, req => new ReservationRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;
            userId = _stringCipher.Decrypt(userId);
            objReservationModel.reservationId = _stringCipher.Decrypt(objReservationModel.reservationId);
            objReservationModel.timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? null;
            var repoReservationResp = await _resvRepository.GetReservationClubTime(userId, objReservationModel);

            if (repoReservationResp == null || repoReservationResp.code != "0")
            {
                resp.code = repoReservationResp == null ? ResponseCodes.Code200_Success : ResponseCodes.Code400_BadRequest;
                resp.message = repoReservationResp == null
                    ? await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound)
                    : await _apiResponseMessageService.GetMessageByCodeAsync(repoReservationResp?.message);
                resp.data = Array.Empty<string>();

                return repoReservationResp == null ? (HttpStatusCode.OK, resp) : (HttpStatusCode.BadRequest, resp);
            }
            ReservationTimeDetailResponse obj = new ReservationTimeDetailResponse();
            obj.reservationId = _stringCipher.Encrypt(repoReservationResp.reservationId);
            obj.visitTime = repoReservationResp.visitTime;
            obj.noOfPeople = Convert.ToInt32(repoReservationResp.noOfPeople);
            var clubtimeList = JsonConvert.DeserializeObject<List<TimeDetail>>(repoReservationResp.clubTimeList.ToString())
                                           ?? new List<TimeDetail>();
            obj.reservationTimeList = clubtimeList.Select(clubtime => new TimeDetail
            {
                isAvailable = Convert.ToBoolean(clubtime.isAvailable),
                time = clubtime.time
            }).ToList();
            
            resp.data = obj;
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUSRES104_ReservationDetailsWithTimeFetchedSuccess);
            return (HttpStatusCode.OK, resp);
        }
        public async Task<(HttpStatusCode, object)> UpdateReservationTime(ReservationTimelModel objReservationTimelModel, string userId)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            objReservationTimelModel.actionIp = _httpContextAccessor.HttpContext?.GetIpAddress();
            var validationResult = await _validationHelper.HandleModelValidationAsync(objReservationTimelModel, req => new ReservationTimeRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;
            userId = _stringCipher.Decrypt(userId);
            objReservationTimelModel.reservationId = _stringCipher.Decrypt(objReservationTimelModel.reservationId);
            ReservationTimeCommon dbRequestCommon = objReservationTimelModel.MapObject<ReservationTimeCommon>();
            dbRequestCommon.timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
            var repoResp = await _resvRepository.UpdateReservationTime(userId, dbRequestCommon);
            if (repoResp.code == "1")
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = repoResp.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }

            if (!string.IsNullOrEmpty(repoResp.Id))
            {
                LineGroupAfterReservationCommon obj = new LineGroupAfterReservationCommon
                {
                    reservationId = objReservationTimelModel.reservationId,
                    flag = "updrestime",
                    timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty
                };
                var message = await _repos.GetMessageToSendInLine(obj);
                var lineClientSecret = _configuration["LineMessagingClientSecret"] ?? string.Empty;
                var sendMessageToLinegroup = await _lineAPIService.SendMessageToLineGroup(lineClientSecret, repoResp?.Id, message);
            }
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message);
            return (HttpStatusCode.OK, resp);
        }

    }
}
