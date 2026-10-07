using hoslog.customer.api.Library;
using hoslog.customer.business.CommonHandler;
using hoslog.customer.common;
using hoslog.customer.common.Models;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Notification;
using hoslog.customer.shared;
using hoslog.customer.shared.Notification;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace hoslog.customer.business.Notification
{
    public class NotificationBusiness : INotificationBusiness
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly StringCipher _stringCipher;
        private readonly INotificationRepository _notificationRepository;
        private readonly URLHelper _uRLHelper;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        public NotificationBusiness(INotificationRepository notificationRepository, IHttpContextAccessor httpContextAccessor, StringCipher stringCipher, URLHelper uRLHelper, ApiResponseMessageService apiResponseMessageService)
        {
            _notificationRepository = notificationRepository;
            _httpContextAccessor = httpContextAccessor;
            _stringCipher = stringCipher;
            _uRLHelper = uRLHelper;
            _apiResponseMessageService = apiResponseMessageService;
        }
        public async Task<(HttpStatusCode, object)> GetNotificationList(int pageNo, AmazonSettingsCommon common)
        {
            var request = new CommonDBRequest
            {
                actionUserId = _stringCipher.Decrypt(_httpContextAccessor.HttpContext?.User?.Claims
                        .FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value)
            };

            var response = await _notificationRepository.GetNotification(request, pageNo);
            var firstResponseItem = response.FirstOrDefault();

            if (firstResponseItem == null || firstResponseItem.code != "0")
            {
                var emptyResponse = new CommonApiResponse
                {
                    code = ResponseCodes.Code200_Success,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound),
                    data = new NotificationResponseModel()
                };
                return (HttpStatusCode.OK, emptyResponse);
            }

            var responseModel = new NotificationResponseModel
            {
                notificationList = response.Select(item => new NotificationDetailCommon
                {
                    notificationId = _stringCipher.Encrypt(item?.notificationId),
                    notificationTo = _stringCipher.Encrypt(item?.notificationTo),
                    notificationType = item.notificationType,
                    notificationSubject = item.notificationSubject,
                    notificationBody = item.notificationBody,
                    notificationStatus = item.notificationStatus,
                    notificationReadStatus = item.notificationReadStatus,
                    allNotificationCount = item.allNotificationCount,
                    dateCategory = item.dateCategory,
                    formattedDate = item.formattedDate,
                    unReadNotificationCount = item.unReadNotificationCount,
                    createdDate = item.createdDate,
                    image = new ImageResponse
                    {
                        alt = item.notificationSubject,
                        path = ImageHandler.ProcessedImage(common, item.notificationImage),
                        description = null
                    },
                    additionalDetail1 = string.IsNullOrEmpty(item?.additionalDetail1) ? string.Empty : _stringCipher.Encrypt(item?.additionalDetail1),
                    additionalDetail2 = string.IsNullOrEmpty(item?.additionalDetail2) ? string.Empty : _stringCipher.Encrypt(item?.additionalDetail2),
                    additionalDetail3 = string.IsNullOrEmpty(item?.additionalDetail3) ? string.Empty : item?.additionalDetail3
                }).ToList(),
                pagination = firstResponseItem.MapObject<PaginationResponse>()
            };

            var successResponse = new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
                data = responseModel
            };
            return (HttpStatusCode.OK, successResponse);
        }

        public async Task<(HttpStatusCode, object)> MarkAllAsReadNotification()
        {
            var response = new CommonApiResponse();
            var userid = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            userid = string.IsNullOrEmpty(userid) ? string.Empty : _stringCipher.Decrypt(userid);
            if (string.IsNullOrEmpty(userid))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = ResponseMessages.Msg400_BadRequest;
                response.data = Array.Empty<string>();
                response.errors = new List<Errors>
                {
                    new Errors{errorCode=ResponseCodes.CodeCUE112_User_Id_is_required,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE112_User_Id_is_required)   }
                };
                return (HttpStatusCode.BadRequest, response);
            }
            var actionIP = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var actionPlatform = _httpContextAccessor?.HttpContext?.Request?.Headers["ActionPlatform"] ?? "WEB";
            var resp = await _notificationRepository.MarkAllAsReadNotificationAsync(userid, actionIP, actionPlatform);
            if (resp.code == "0")
            {
                string notificationIds = string.Empty;
                if (!string.IsNullOrEmpty(resp?.notificationIds))
                {
                    var notificationIdsData = resp.notificationIds.Split(',').Select(x => x = _stringCipher.Encrypt(x)).ToArray();
                    notificationIds = notificationIdsData.Count() > 0 ? string.Join(",", notificationIdsData) : string.Empty;
                }
                response.code = ResponseCodes.Code200_Success;
                response.message = !string.IsNullOrEmpty(resp.message) ? await _apiResponseMessageService.GetMessageByCodeAsync(resp.message) : ResponseMessages.Msg200_Success;
                response.data = new { notificationIds };
                return (HttpStatusCode.OK, response);
            }
            else if (resp.code == "1")
            {
                response.errors = new List<Errors>
                {
                    new Errors{errorCode=resp.message,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(resp.message)   }
                };
            }
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = ResponseMessages.Msg400_BadRequest;
            response.data = Array.Empty<string>();
            return (HttpStatusCode.BadRequest, response);
        }

        public async Task<(HttpStatusCode, object)> MarkAsReadNotification(NotificationMarkAsReadRequestCommon requestMapped)
        {
            var response = new CommonApiResponse();
            requestMapped.actionUserId = _stringCipher.Decrypt(_httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value);
            requestMapped.notificationId = _stringCipher.Decrypt(requestMapped.notificationId);
            if (string.IsNullOrEmpty(requestMapped.actionUserId) || string.IsNullOrEmpty(requestMapped.notificationId))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = ResponseMessages.Msg400_BadRequest;
                response.data = Array.Empty<string>();
                response.errors = new List<Errors>
                {
                    new Errors{errorCode=ResponseCodes.CodeCUE100_Invalid_request,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE100_Invalid_request)}
                };
                return (HttpStatusCode.BadRequest, response);
            }
            requestMapped.actionIP = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            requestMapped.actionPlatform = _httpContextAccessor?.HttpContext?.Request?.Headers["ActionPlatform"] ?? "WEB";
            var resp = await _notificationRepository.MarkAsReadNotificationAsync(requestMapped);
            if (resp.code == "0")
            {
                response.code = ResponseCodes.Code200_Success;
                response.message = !string.IsNullOrEmpty(resp.message) ? await _apiResponseMessageService.GetMessageByCodeAsync(resp.message) : ResponseMessages.Msg200_Success;
                response.data = Array.Empty<string>();
                return (HttpStatusCode.OK, response);
            }
            else
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = ResponseMessages.Msg400_BadRequest;
                response.errors = new List<Errors>
                {
                    new Errors{errorCode=resp.message,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(resp.message)   }
                };
                response.data = Array.Empty<string>();
            }
            return (HttpStatusCode.BadRequest, response);
        }

        public async Task<(HttpStatusCode, object)> CheckUnreadNotification()
        {
            var request = new CommonDBRequest
            {
                actionUserId = _stringCipher.Decrypt(_httpContextAccessor.HttpContext?.User?.Claims
                        .FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value)
            };

            var response = await _notificationRepository.CheckUnreadNotification(request);

            if (response == null || response.code != "0")
            {
                var emptyResponse = new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest),

                };
                return (HttpStatusCode.OK, emptyResponse);
            }

            var responseModel = new UnreadNotificationStatusResponse
            {
                notificationUnreadStatus = Convert.ToBoolean(response.notificationUnreadStatus)
            };

            var successResponse = new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
                data = responseModel
            };
            return (HttpStatusCode.OK, successResponse);
        }
    }
}
