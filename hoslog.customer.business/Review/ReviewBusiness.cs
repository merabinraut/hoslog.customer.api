using hoslog.customer.business.CommonHandler;
using hoslog.customer.common;
using hoslog.customer.common.Models;
using hoslog.customer.common.Models.ReviewModel;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Review;
using hoslog.customer.shared;
using hoslog.customer.shared.Review;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace hoslog.customer.business.Review
{
    public class ReviewBusiness : IReviewBusiness
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly CommonResponseHandler _commonResponseHandler;
        private readonly StringCipher _stringCipher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ValidationHelper _validationHelper;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        public ReviewBusiness(IReviewRepository reviewRepository, StringCipher stringCipher, IHttpContextAccessor httpContextAccessor, ValidationHelper validationHelper, CommonResponseHandler commonResponseHandler, ApiResponseMessageService apiResponseMessageService)
        {
            _reviewRepository = reviewRepository;
            _commonResponseHandler = commonResponseHandler;
            _stringCipher = stringCipher;
            _httpContextAccessor = httpContextAccessor;
            _validationHelper = validationHelper;
            _apiResponseMessageService = apiResponseMessageService;
        }

        public async Task<(HttpStatusCode, object)> GetUserReviewList(PaginationRequestModel objPaginationRequestModel, string userId, AmazonSettingsCommon common, string type = "all")
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };

            var repoResp = await _reviewRepository.GetUserReviewList(objPaginationRequestModel, _stringCipher.Decrypt(userId), type);
            if (repoResp == null || repoResp.Count <= 0)
            {
                resp.data = new ReviewCommonData();
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
                return (HttpStatusCode.OK, resp);
            }

            var reviewDetail = new ReviewCommonData
            {
                pagination = repoResp.Select(review => new PaginationResponse
                {
                    pageNo = objPaginationRequestModel.pageNo,
                    totalRecords = repoResp.FirstOrDefault()?.totalRecords,
                    recordPerPage = repoResp.FirstOrDefault()?.recordPerPage,
                }).FirstOrDefault(),
                reviewList = repoResp.Select(review => new ReviewCommonResponse
                {
                    clubId = _stringCipher.Encrypt(review.clubId),
                    clubLocationID = _stringCipher.Encrypt(review.clubLocationID),
                    clubNameEng = review.clubNameEng,
                    clubName = review.clubNameJap,
                    clubCode = review.clubCode,
                    commentedOn = review.commentedOn,
                    ratingScale = !string.IsNullOrEmpty(review.ratingScale) ? int.Parse(review.ratingScale) : 0,
                    image = new ImageResponse
                    {
                        alt = review.clubNameJap,
                        path = ImageHandler.ProcessedImage(common, review.image),
                        description = "review image of club"
                    },
                    type = review.type,
                    reservationId = _stringCipher.Encrypt(review.reservationId)
                }).ToList()
            };
            resp.data = reviewDetail;
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetchedSuccess);
            return (HttpStatusCode.OK, resp);
        }

        public async Task<(HttpStatusCode, object)> GetReservationDetails(string reservationId, string userId, AmazonSettingsCommon mappedAwsSetting)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            if (string.IsNullOrEmpty(reservationId) || string.IsNullOrEmpty(userId))
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>();
            }
            else
            {
                ReviewReservationRequestModel dbRequest = new ReviewReservationRequestModel()
                {
                    reservationId = _stringCipher.Decrypt(reservationId),
                    userId = _stringCipher.Decrypt(userId)
                };
                var dbResponse = await _reviewRepository.GetReservationDetail(dbRequest);
                if (dbResponse == null)
                {
                    return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>();
                }
                else
                {
                    if (dbResponse[0].code == "1")
                    {
                        resp.code = ResponseCodes.Code400_BadRequest;
                        resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                        resp.data = Array.Empty<string>();
                        resp.errors = new List<Errors>
                        {
                            new Errors
                            {
                                errorCode = dbResponse[0].message,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse[0].message)
                            }
                        };
                        return (HttpStatusCode.OK, resp);
                    }
                    var mappedReservationDetail = dbResponse?
                        .Select(reviewList => new ReviewReservationResponseModel
                        {
                            clubId = _stringCipher.Encrypt(reviewList.clubId),
                            customerId = _stringCipher.Encrypt(reviewList.customerId),
                            reservationId = _stringCipher.Encrypt(reviewList.reservationId),
                            clubLocationName = reviewList.clubLocationName,
                            clubLocationNameFull = reviewList.clubLocationNameFull,
                            clubNameEnglish = reviewList.clubNameEnglish,
                            clubNameJapanese = reviewList.clubNameJapanese,
                            noOfPeople = reviewList.noOfPeople,
                            paymentMethod = reviewList.paymentMethod,
                            price = reviewList.price,
                            reservationDateTime = reviewList.reservationDateTime,
                            planName = reviewList.planName,
                            totalAmount = reviewList.totalAmount,
                            usedPoint = reviewList.usedPoint,
                            clubLogoImageResp = new ImageResponse
                            {
                                alt = reviewList.clubNameEnglish,
                                description = "club logo image",
                                path = ImageHandler.ProcessedImage(
                                    mappedAwsSetting,
                                    reviewList.clubLogo
                                )
                            },
                            landLineNumber = reviewList.landLineNumber
                        })
                        .ToList() ?? new List<ReviewReservationResponseModel>();

                    resp.data = mappedReservationDetail;
                    resp.code = ResponseCodes.Code200_Success;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse[0].message);
                    return (HttpStatusCode.OK, resp);
                }
            }
        }

        public async Task<(HttpStatusCode, object)> GetReviewHostListViaClubId(AmazonSettingsCommon mappedAwsSetting, string clubId)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            if (string.IsNullOrEmpty(clubId))
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>();
            }
            else
            {
                ReviewStep2CommonModel reviewStep2Common = new ReviewStep2CommonModel();
                string decryptClubId = _stringCipher.Decrypt(clubId);
                var dbResponse = await _reviewRepository.GetReviewHostListViaClubId(decryptClubId);
                reviewStep2Common.getClubShortDetail = await GetClubShortDetail(decryptClubId);
                reviewStep2Common.getReviewHostList = dbResponse.MapObjects<ReviewHostListModel>();
                if (dbResponse == null)
                {
                    resp.code = ResponseCodes.Code200_Success;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
                    resp.data = Array.Empty<string>();
                    return (HttpStatusCode.OK, resp);
                }
                else
                {
                    var mappedHostList = reviewStep2Common.getReviewHostList.Select(hostList => new ReviewHostListModel
                    {
                        clubId = _stringCipher.Encrypt(hostList.clubId),
                        hostId = _stringCipher.Encrypt(hostList.hostId),
                        hostImageResponse = new ImageResponse
                        {
                            alt = hostList.hostName,
                            description = "host image",
                            path = ImageHandler.ProcessedImage(mappedAwsSetting, hostList.hostImage),
                        },
                        hostName = hostList.hostName,
                        hostNameJapanese = hostList.hostNameJapanese,
                        hostPosition = hostList.hostPosition,
                        hostRank = hostList.hostRank,
                    }).ToList();
                    var mappedClubDetail = reviewStep2Common.getClubShortDetail.Select(clubObj => new ReviewClubShortDetailModel
                    {
                        clubId = _stringCipher.Encrypt(clubObj.clubId),
                        clubName = clubObj.clubName,
                        clubNameJapanese = clubObj.clubNameJapanese,
                        clubLocationName = clubObj.clubLocationName,
                        clubLogoResponse = new ImageResponse
                        {
                            alt = clubObj.clubName,
                            description = "club logo image",
                            path = ImageHandler.ProcessedImage(mappedAwsSetting, clubObj.clubLogo)
                        }
                    }).ToList();
                    resp.data = new
                    {
                        hostList = mappedHostList,
                        clubdetail = mappedClubDetail
                    };
                    resp.code = ResponseCodes.Code200_Success;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetchedSuccess);
                    return (HttpStatusCode.OK, resp);
                }
            }
        }
        private async Task<IList<ReviewClubShortDetailModel>> GetClubShortDetail(string? clubId)
        {
            var dbResponse = await _reviewRepository.GetClubShortDetail(clubId);
            return dbResponse;
        }

        public async Task<(HttpStatusCode, object)> GetReviewRemarkList(string clubId, AmazonSettingsCommon mappedAwsSetting)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            if (string.IsNullOrEmpty(clubId))
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>();
            }
            else
            {
                ReviewStep3CommonModel reviewStep3Common = new ReviewStep3CommonModel();
                string decryptClubId = _stringCipher.Decrypt(clubId);
                var dbResponse = await _reviewRepository.GetReviewRemarkList();
                reviewStep3Common.getReviewRemarkList = dbResponse.MapObjects<ReviewRemarkListModel>();
                reviewStep3Common.getClubShortDetail = await GetClubShortDetail(decryptClubId);
                if (dbResponse == null)
                {
                    resp.code = ResponseCodes.Code200_Success;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
                    resp.data = Array.Empty<string>();
                    return (HttpStatusCode.OK, resp);
                }
                else
                {
                    var mappedReviewStep3 = reviewStep3Common.getReviewRemarkList.Select(remarklist => new ReviewRemarkListModel
                    {
                        remarkId = _stringCipher.Encrypt(remarklist.remarkId),
                        remarkLabelEnglish = remarklist.remarkLabelEnglish,
                        remarkLabelJapanese = remarklist.remarkLabelJapanese,
                        remarkTypeEnglish = remarklist.remarkTypeEnglish,
                        remarkTypeJapanese = remarklist.remarkTypeJapanese
                    }).ToList();
                    var mappedClubDetail = reviewStep3Common.getClubShortDetail.Select(clubObj => new ReviewClubShortDetailModel
                    {
                        clubId = _stringCipher.Encrypt(clubObj.clubId),
                        clubLocationName = clubObj.clubLocationName,
                        clubLogoResponse = new ImageResponse
                        {
                            alt = clubObj.clubName,
                            description = "club logo image",
                            path = ImageHandler.ProcessedImage(mappedAwsSetting, clubObj.clubLogo)
                        },
                        clubName = clubObj.clubName,
                        clubNameJapanese = clubObj.clubNameJapanese
                    }).ToList();
                    resp.code = ResponseCodes.Code200_Success;
                    resp.data = new
                    {
                        remarkList = mappedReviewStep3,
                        clubDetail = mappedClubDetail
                    };
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUSREV101_ReviewRemarkListFetchedSuccess);
                    return (HttpStatusCode.OK, resp);
                }
            }
        }

        public async Task<(HttpStatusCode, object)> GetReviewQNAList(string clubId, AmazonSettingsCommon mappedAwsSetting)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            if (string.IsNullOrEmpty(clubId))
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>();
            }
            else
            {
                ReviewStep4CommonModel reviewStep4Common = new ReviewStep4CommonModel();
                string decryptClubId = _stringCipher.Decrypt(clubId);
                var dbResponseQuestion = await _reviewRepository.GetReviewDichotomousQuestionList();
                var dbResponseAnswer = await _reviewRepository.GetReviewDichotomousAnswerList();
                reviewStep4Common.getReviewDichotomousQuestion = dbResponseQuestion.MapObjects<ReviewDichotomousQuestionModel>();
                reviewStep4Common.getReviewDichotomousAnswer = dbResponseAnswer.MapObjects<ReviewDichotomousAnswerModel>();
                reviewStep4Common.getClubShortDetail = await GetClubShortDetail(decryptClubId);
                if (dbResponseQuestion == null && dbResponseAnswer == null)
                {
                    resp.code = ResponseCodes.Code200_Success;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
                    resp.data = Array.Empty<string>();
                    return (HttpStatusCode.OK, resp);
                }
                else
                {
                    var mappedQuestion = reviewStep4Common.getReviewDichotomousQuestion.Select(questionList => new ReviewDichotomousQuestionModel
                    {
                        remarkId = _stringCipher.Encrypt(questionList.remarkId),
                        remarkLabelEnglish = questionList.remarkLabelEnglish,
                        remarkLabelJapanese = questionList.remarkLabelJapanese

                    }).ToList();
                    var mappedAnswer = reviewStep4Common.getReviewDichotomousAnswer.Select(answerList => new ReviewDichotomousAnswerModel
                    {
                        remarkId = _stringCipher.Encrypt(answerList.remarkId),
                        remarkLabelJapanese = answerList.remarkLabelJapanese,
                        remarkLabelEnglish = answerList.remarkLabelEnglish
                    }).ToList();
                    var mappedClubDetail = reviewStep4Common.getClubShortDetail.Select(clubObj => new ReviewClubShortDetailModel
                    {
                        clubId = _stringCipher.Encrypt(clubObj.clubId),
                        clubLocationName = clubObj.clubLocationName,
                        clubName = clubObj.clubName,
                        clubNameJapanese = clubObj.clubNameJapanese,
                        clubLogoResponse = new ImageResponse
                        {
                            alt = clubObj.clubName,
                            description = "club logo image",
                            path = ImageHandler.ProcessedImage(mappedAwsSetting, clubObj.clubLogo)
                        }
                    }).ToList();
                    resp.code = ResponseCodes.Code200_Success;
                    resp.data = new
                    {
                        dichotomous_questions = mappedQuestion,
                        dichotomous_answer = mappedAnswer,
                        club_detail = mappedClubDetail
                    };
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUSREV102_DichotomousQuestionsAndAnswerListFetchedSuccess);
                    return (HttpStatusCode.OK, resp);
                }
            }
        }

        public async Task<(HttpStatusCode, object)> GetReviewDetail(string clubId, AmazonSettingsCommon mappedAwsSetting)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            if (string.IsNullOrEmpty(clubId))
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>();
            }
            else
            {
                string decryptClubId = _stringCipher.Decrypt(clubId);
                var dbResponse = await GetClubShortDetail(decryptClubId);
                var mappedClubDetail = dbResponse.MapObjects<ReviewClubShortDetailModel>();
                if (dbResponse == null)
                {
                    resp.code = ResponseCodes.Code200_Success;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
                    resp.data = Array.Empty<string>();
                    return (HttpStatusCode.OK, resp);
                }
                else
                {
                    mappedClubDetail = mappedClubDetail.Select(clubObj => new ReviewClubShortDetailModel
                    {
                        clubId = _stringCipher.Encrypt(clubObj.clubId),
                        clubLocationName = clubObj.clubLocationName,
                        clubLogoResponse = new ImageResponse
                        {
                            alt = clubObj.clubName,
                            description = "club logo image",
                            path = ImageHandler.ProcessedImage(mappedAwsSetting, clubObj.clubLogo)
                        },
                        clubName = clubObj.clubName,
                        clubNameJapanese = clubObj.clubNameJapanese,
                    }).ToList();
                    resp.code = ResponseCodes.Code200_Success;
                    resp.data = mappedClubDetail;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetchedSuccess);
                    return (HttpStatusCode.OK, resp);
                }
            }
        }

        public async Task<(HttpStatusCode, object)> ManageReview(ManageReviewRequestModel requestModel)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            requestModel.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
            var validationResult = await _validationHelper.HandleModelValidationAsync(requestModel, request => new ManageReviewRequestValidatorModel().Validate(request));
            if (validationResult.Item1 != HttpStatusCode.OK)
            {
                return validationResult;
            }
            else
            {
                ReviewRequestModel dbRequest = new ReviewRequestModel();
                dbRequest.userId = requestModel.userId != null ? _stringCipher.Decrypt(requestModel.userId) : null;
                dbRequest.clubId = requestModel.clubId != null ? _stringCipher.Decrypt(requestModel.clubId) : null;
                dbRequest.mvpHostId = requestModel.mvpHostId != null ? _stringCipher.Decrypt(requestModel.mvpHostId) : null;
                dbRequest.reservationId = requestModel.reservationId != null ? _stringCipher.Decrypt(requestModel.reservationId) : null;
                dbRequest.reservationId = requestModel.reservationId != null ? _stringCipher.Decrypt(requestModel.reservationId) : null;

                // Convert lists properly using string.Join() to create a string representation
                dbRequest.hostList = requestModel.hostIdList != null
                    ? string.Join(",", requestModel.hostIdList.Select(x => _stringCipher.Decrypt(x)))
                    : null;

                dbRequest.remarkList = requestModel.remarkIdList != null
                    ? string.Join(",", requestModel.remarkIdList.Select(x => _stringCipher.Decrypt(x)))
                    : null;

                dbRequest.intent = Convert.ToInt16(requestModel.intent != null ? _stringCipher.Decrypt(requestModel.intent) : null);
                dbRequest.remark = requestModel.remark;
                string[]? DichotomousQAIdList = requestModel.dichotomousQAIdList;
                if (DichotomousQAIdList != null)
                {
                    for (int i = 0; i < DichotomousQAIdList.Length; i++)
                    {
                        string[] parts = DichotomousQAIdList[i].Split(':');
                        if (parts.Length >= 2)
                        {
                            DichotomousQAIdList[i] = $"{_stringCipher.Decrypt(parts[0])}:{_stringCipher.Decrypt(parts[1])}";
                        }
                    }
                    dbRequest.dichotomousList = string.Join(",", DichotomousQAIdList);
                }
                else
                {
                    dbRequest.dichotomousList = null;
                }
                dbRequest.ratingScale = requestModel.ratingScale;
                var dbResponse = await _reviewRepository.ManageReview(dbRequest);
                if (dbResponse.code == "0")
                {
                    resp.code = ResponseCodes.Code200_Success;
                    resp.message = dbResponse.message;
                    resp.data = Array.Empty<string>();
                    return (HttpStatusCode.OK, resp);
                }
                else
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                    resp.data = Array.Empty<string>();
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = dbResponse.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse.message)
                        }
                    };
                    return (HttpStatusCode.BadRequest, resp);
                }
            }
        }
    }
}
