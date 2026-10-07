using Dapper;
using hoslog.customer.shared;
using hoslog.customer.shared.Inquiries;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Inquiries
{
    public class InquiriesRepository : IInquiriesRepository
    {
        private readonly RepositoryDao _dao;

        public InquiriesRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<CommonDBResponse> PostInquiriesAsync(InquiriesRequestCommon request)
        {
            string proc = "apiproc_cp_request_inquiry";
            var parameters = new DynamicParameters();
            parameters.Add("@emailAddress", request.emailAddress);
            parameters.Add("@phoneNumber", request.mobileNumber);
            parameters.Add("@fullName", request.name);
            parameters.Add("@inquiryType", request.inquiryType);
            parameters.Add("@subject", request.subject);
            parameters.Add("@message", request.description);
            parameters.Add("@attachments", request.attachment);
            parameters.Add("@companyName", request.companyName);
            parameters.Add("@actionBy", request.actionUserId);
            parameters.Add("@actionPlatfrom", request.actionPlatform);
            parameters.Add("@actionIP", request.actionIP);
            var dbResp = await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
            return dbResp;
        }
    }
}
