using Dapper;
using hoslog.customer.common.Models.Notice;
using hoslog.customer.shared.Notice;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Notice
{
    public class NoticeRepository : INoticeRepository
    {
        private readonly RepositoryDao _dao;

        public NoticeRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<List<NoticeDetailResponseCommon>> GetClubNoticeAsync(NoticeModel request)
        {
            string proc = "apiproc_cp_get_notice_list ";
            var parameters = new DynamicParameters();
            parameters.Add("@clubId", request.clubId);
            parameters.Add("@pageNo", request.pageNo);
            parameters.Add("@clubCode", request.clubCode);
            var dbResp = await _dao.ExecuteAsyncList<NoticeDetailResponseCommon>(proc, parameters);
            return dbResp;
        }
    }
}
