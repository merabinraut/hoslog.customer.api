using hoslog.customer.common.Service;
using hoslog.customer.shared;
using System.Net;

namespace hoslog.customer.common.Utilities
{
    public class CommonResponseHandler
    {
        private readonly ApiResponseMessageService _apiResponseMessageService;
        public CommonResponseHandler(ApiResponseMessageService apiResponseMessageService)
        {
            _apiResponseMessageService = apiResponseMessageService;
        }

        public async Task<(HttpStatusCode, T)> HandleBadRequestResponse<T>(string responseMessage = null, string responseDetailMessage = null, List<Errors> fieldErrors = null) where T : CommonApiResponse
        {
            var instance = Activator.CreateInstance<T>();
            instance.code = ResponseCodes.Code400_BadRequest;
            //instance.ResponseStatus = ResponseStatuses.Error;

            instance.message = responseMessage;

            if (responseMessage is  null)
            instance.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);


            //if (responseDetailMessage is not null)
            //    instance.ResponseDetailMessage = responseDetailMessage;

            if (fieldErrors is not null)
                instance.errors = fieldErrors;

            return (HttpStatusCode.BadRequest, instance);
        }

        public async Task<(HttpStatusCode, T)> HandleBadGatewayResponse<T>(string responseMessage = null, string responseDetailMessage = null) where T : CommonApiResponse
        {
            var instance = Activator.CreateInstance<T>();
            instance.code = ResponseCodes.Code502_BadGateway;
            //instance.ResponseStatus = ResponseStatuses.Error;
            instance.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg502_BadGateway);

            if (responseMessage is not null)
                instance.message = responseMessage;

            //if (responseDetailMessage is not null)
            //    instance.ResponseDetailMessage = responseDetailMessage;

            return (HttpStatusCode.BadGateway, instance);
        }

        public async Task<(HttpStatusCode, T)> HandleGatewayTimeoutResponse<T>(string responseMessage = null, string responseDetailMessage = null) where T : CommonApiResponse
        {
            var instance = Activator.CreateInstance<T>();
            instance.code = ResponseCodes.Code504_GatewayTimeout;
            //instance.ResponseStatus = ResponseStatuses.Error;
            instance.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg504_GatewayTimeout);

            if (responseMessage is not null)
                instance.message = responseMessage;

            //if (responseDetailMessage is not null)
            //    instance.ResponseDetailMessage = responseDetailMessage;

            return (HttpStatusCode.GatewayTimeout, instance);
        }

        public async Task<(HttpStatusCode, T)> HandleInternalServerErrorResponse<T>(string responseMessage = null, string responseDetailMessage = null) where T : CommonApiResponse
        {
            var instance = Activator.CreateInstance<T>();
            instance.code = ResponseCodes.Code500_InternalServerError;
            //instance.ResponseStatus = ResponseStatuses.Error;
            instance.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg500_InternalServerError);

            if (responseMessage is not null)
                instance.message = responseMessage;

            //if (responseDetailMessage is not null)
            //    instance.ResponseDetailMessage = responseDetailMessage;

            return (HttpStatusCode.BadRequest, instance);
        }

        public async Task<(HttpStatusCode, object)> HandleInvalidSignatureTokenResponse<T>() where T : CommonApiResponse
        {
            var instance = Activator.CreateInstance<T>();
            instance.code = ResponseCodes.CodeE601_InvalidSignatureToken;
            //instance.ResponseStatus = ResponseStatuses.Error;
            instance.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.MsgE601_InvalidSignatureToken);

            return (HttpStatusCode.BadRequest, instance);
        }

        public async Task<(HttpStatusCode, object)> HandleInvalidProcessIdResponse<T>() where T : CommonApiResponse
        {
            var instance = Activator.CreateInstance<T>();
            instance.code = ResponseCodes.CodeE602_InvalidProcessId;
            //instance.ResponseStatus = ResponseStatuses.Error;
            instance.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.MsgE602_InvalidProcessId);

            return (HttpStatusCode.BadRequest, instance);
        }

        public async Task<(HttpStatusCode, T)> HandleInvalidEncryptionErrorResponse<T>(string responseMessage = null, string responseDetailMessage = null) where T : CommonApiResponse
        {
            var instance = Activator.CreateInstance<T>();
            instance.code = ResponseCodes.CodeE603_InvalidEncryption;
            //instance.ResponseStatus = ResponseStatuses.Error;
            instance.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.MsgE603_InvalidEncryption);
            //instance.ResponseDetailMessage = "Possible reasons include incorrect encryption key, data corruption, or encryption algorithm mismatch. Please review the provided data and encryption parameters and try again.";

            if (responseMessage is not null)
                instance.message = responseMessage;

            //if (responseDetailMessage is not null)
            //    instance.ResponseDetailMessage = responseDetailMessage;

            return (HttpStatusCode.BadRequest, instance);
        }
    }
}
