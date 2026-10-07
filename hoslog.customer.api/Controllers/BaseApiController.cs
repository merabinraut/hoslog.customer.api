using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hoslog.customer.api.Controllers
{
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected virtual IActionResult HandleResponseFromStatusCode<TResponse>(HttpStatusCode statusCode, TResponse response)
        {
            return statusCode switch
            {
                HttpStatusCode.OK => StatusCode(StatusCodes.Status200OK, response),
                HttpStatusCode.Created => StatusCode(StatusCodes.Status201Created, response),
                HttpStatusCode.NonAuthoritativeInformation => StatusCode(StatusCodes.Status203NonAuthoritative, response),
                HttpStatusCode.NoContent => StatusCode(StatusCodes.Status204NoContent, response),

                HttpStatusCode.BadRequest => StatusCode(StatusCodes.Status400BadRequest, response),
                HttpStatusCode.Unauthorized => StatusCode(StatusCodes.Status401Unauthorized, response),
                HttpStatusCode.PaymentRequired => StatusCode(StatusCodes.Status402PaymentRequired, response),
                HttpStatusCode.Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
                HttpStatusCode.NotFound => StatusCode(StatusCodes.Status404NotFound, response),
                HttpStatusCode.MethodNotAllowed => StatusCode(StatusCodes.Status405MethodNotAllowed, response),
                HttpStatusCode.NotAcceptable => StatusCode(StatusCodes.Status406NotAcceptable, response),
                HttpStatusCode.Conflict => StatusCode(StatusCodes.Status409Conflict, response),

                HttpStatusCode.InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, response),
                HttpStatusCode.NotImplemented => StatusCode(StatusCodes.Status501NotImplemented, response),
                HttpStatusCode.BadGateway => StatusCode(StatusCodes.Status502BadGateway, response),
                HttpStatusCode.ServiceUnavailable => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
                HttpStatusCode.GatewayTimeout => StatusCode(StatusCodes.Status504GatewayTimeout, response),

                _ => StatusCode(StatusCodes.Status400BadRequest, response),
            };
        }
    }
}
