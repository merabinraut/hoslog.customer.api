// using hoslog.customer.common.Models;
// using hoslog.customer.shared;
// using System.Net;

// namespace hoslog.customer.business.CommonHandler
// {
//     public class CommonResponseHandler
//     {
//         public (HttpStatusCode, T) HandleBadRequestResponse<T>(string responseMessage = null, string responseDetailMessage = null, List<Errors> fieldErrors = null) where T : CommonApiResponse
//         {
//             var instance = Activator.CreateInstance<T>();
//             instance.code = ResponseCodes.Code400_BadRequest;
//             //instance.ResponseStatus = ResponseStatuses.Error;
//             instance.message = ResponseMessages.Msg400_BadRequest;

//             if (responseMessage is not null)
//                 instance.message = responseMessage;

//             //if (responseDetailMessage is not null)
//             //    instance.ResponseDetailMessage = responseDetailMessage;

//             if (fieldErrors is not null)
//                 instance.errors = fieldErrors;

//             return (HttpStatusCode.BadRequest, instance);
//         }

//         public (HttpStatusCode, T) HandleBadGatewayResponse<T>(string responseMessage = null, string responseDetailMessage = null) where T : CommonApiResponse
//         {
//             var instance = Activator.CreateInstance<T>();
//             instance.code = ResponseCodes.Code502_BadGateway;
//             //instance.ResponseStatus = ResponseStatuses.Error;
//             instance.message = ResponseMessages.Msg502_BadGateway;

//             if (responseMessage is not null)
//                 instance.message = responseMessage;

//             //if (responseDetailMessage is not null)
//             //    instance.ResponseDetailMessage = responseDetailMessage;

//             return (HttpStatusCode.BadGateway, instance);
//         }

//         public (HttpStatusCode, T) HandleGatewayTimeoutResponse<T>(string responseMessage = null, string responseDetailMessage = null) where T : CommonApiResponse
//         {
//             var instance = Activator.CreateInstance<T>();
//             instance.code = ResponseCodes.Code504_GatewayTimeout;
//             //instance.ResponseStatus = ResponseStatuses.Error;
//             instance.message = ResponseMessages.Msg504_GatewayTimeout;

//             if (responseMessage is not null)
//                 instance.message = responseMessage;

//             //if (responseDetailMessage is not null)
//             //    instance.ResponseDetailMessage = responseDetailMessage;

//             return (HttpStatusCode.GatewayTimeout, instance);
//         }

//         public (HttpStatusCode, T) HandleInternalServerErrorResponse<T>(string responseMessage = null, string responseDetailMessage = null) where T : CommonApiResponse
//         {
//             var instance = Activator.CreateInstance<T>();
//             instance.code = ResponseCodes.Code500_InternalServerError;
//             //instance.ResponseStatus = ResponseStatuses.Error;
//             instance.message = ResponseMessages.Msg500_InternalServerError;

//             if (responseMessage is not null)
//                 instance.message = responseMessage;

//             //if (responseDetailMessage is not null)
//             //    instance.ResponseDetailMessage = responseDetailMessage;

//             return (HttpStatusCode.BadRequest, instance);
//         }

//         public (HttpStatusCode, object) HandleInvalidSignatureTokenResponse<T>() where T : CommonApiResponse
//         {
//             var instance = Activator.CreateInstance<T>();
//             instance.code = ResponseCodes.CodeE601_InvalidSignatureToken;
//             //instance.ResponseStatus = ResponseStatuses.Error;
//             instance.message = ResponseMessages.MsgE601_InvalidSignatureToken;

//             return (HttpStatusCode.BadRequest, instance);
//         }

//         public (HttpStatusCode, object) HandleInvalidProcessIdResponse<T>() where T : CommonApiResponse
//         {
//             var instance = Activator.CreateInstance<T>();
//             instance.code = ResponseCodes.CodeE602_InvalidProcessId;
//             //instance.ResponseStatus = ResponseStatuses.Error;
//             instance.message = ResponseMessages.MsgE602_InvalidProcessId;

//             return (HttpStatusCode.BadRequest, instance);
//         }

//         public (HttpStatusCode, T) HandleInvalidEncryptionErrorResponse<T>(string responseMessage = null, string responseDetailMessage = null) where T : CommonApiResponse
//         {
//             var instance = Activator.CreateInstance<T>();
//             instance.code = ResponseCodes.CodeE603_InvalidEncryption;
//             //instance.ResponseStatus = ResponseStatuses.Error;
//             instance.message = ResponseMessages.MsgE603_InvalidEncryption;
//             //instance.ResponseDetailMessage = "Possible reasons include incorrect encryption key, data corruption, or encryption algorithm mismatch. Please review the provided data and encryption parameters and try again.";

//             if (responseMessage is not null)
//                 instance.message = responseMessage;

//             //if (responseDetailMessage is not null)
//             //    instance.ResponseDetailMessage = responseDetailMessage;

//             return (HttpStatusCode.BadRequest, instance);
//         }


//     }
// }
