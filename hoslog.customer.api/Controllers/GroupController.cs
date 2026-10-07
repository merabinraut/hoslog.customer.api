using Asp.Versioning;
using hoslog.customer.business.Group;
using hoslog.customer.common.Models.Group;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class GroupController : BaseApiController
    {
        private readonly IGroupBusiness _business;
        public GroupController(IGroupBusiness business)
        {
            _business = business;
        }

        [HttpGet, Route("get-group-list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroupList(string freeWord = "", string vowel = "", int pageNo = 1)
        {
            var (statusCode, response) = await _business.GetGroupList(freeWord, vowel, pageNo);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        [HttpGet, Route("get-group-detail")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroupDetail(string clubId = "", string groupId = "")
        {
            var requestModel = new GroupRequestModel();
            requestModel.clubId = !string.IsNullOrEmpty(clubId) ? clubId : string.Empty;
            requestModel.groupId = !string.IsNullOrEmpty(groupId) ? groupId : string.Empty;
            var (statusCode, response) = await _business.GetGroupDetail(requestModel);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}

