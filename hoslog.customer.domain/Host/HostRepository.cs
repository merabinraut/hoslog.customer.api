using Dapper;
using hoslog.customer.common.Host;
using hoslog.customer.shared.Host;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Host
{
    public class HostRepository : IHostRepository
    {
        private readonly RepositoryDao _dao;

        public HostRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }
        public async Task<(GetHostListDetailsCommon, List<Skills>, List<Personality>, List<HostGallaryImage>)> GetHostDetailsAsync(string hostId, string hostCode, string userId)
        {
            const string ProcHost = "apiproc_cp_get_specific_host_detail";
            const string ProcSkills = "apiproc_cp_get_host_skills_list";
            const string ProcPersonality = "apiproc_cp_get_host_personality_list";
            const string ProcHostImage = "apiproc_cp_get_host_images";

            var baseParams = new DynamicParameters();
            baseParams.Add("@hostId", hostId);
            baseParams.Add("@hostCode", hostCode);

            var hostParams = new DynamicParameters(baseParams);
            hostParams.Add("@userId", userId);

            var host = await _dao.ExecuteAsync<GetHostListDetailsCommon>(ProcHost, hostParams)
                                 .ConfigureAwait(false);

            if (host == null)
            {
                return (null,
                    new List<Skills>(),
                    new List<Personality>(),
                    new List<HostGallaryImage>());
            }

            var skillsTask = _dao.ExecuteAsyncList<Skills>(ProcSkills, baseParams);
            var personalityTask = _dao.ExecuteAsyncList<Personality>(ProcPersonality, baseParams);
            var hostImageTask = _dao.ExecuteAsyncList<HostGallaryImage>(ProcHostImage, baseParams);

            await Task.WhenAll(skillsTask, personalityTask, hostImageTask).ConfigureAwait(false);

            return (host, skillsTask.Result, personalityTask.Result, hostImageTask.Result);
        }

        public async Task<List<HostDetailResponseCommon>> GetHostListAsync(HostDetailRequestCommon requestCommon)
        {
            string proc = "apiproc_cp_get_host_list ";
            var parameters = new DynamicParameters();
            parameters.Add("@clubId", requestCommon.clubId);
            parameters.Add("@clubCode", requestCommon.clubCode);
            parameters.Add("@userId", requestCommon.userId);
            var dbResp = await _dao.ExecuteAsyncList<HostDetailResponseCommon>(proc, parameters);
            return dbResp;
        }

        public async Task<IList<HostDetailResponseCommon>> GetRankedHostListAsync(HostDetailRequestCommon request)
        {
            string sp_name = "[dbo].[apiproc_cp_get_ranked_host_list]";
            var param = new DynamicParameters();
            param.Add("@clubId", request.clubId);
            param.Add("@clubCode", request.clubCode);
            param.Add("@userId", request.userId);
            var dbResponse = await _dao.ExecuteAsyncList<HostDetailResponseCommon>(sp_name, param);
            return dbResponse;
        }

        public async Task<List<ScheduleResponseCommon>> GetScheduleListAsync(ScheduleDetailsRequestCommon requesMapped)
        {
            string proc = "apiproc_cp_get_schedule_details ";
            var parameters = new DynamicParameters();
            parameters.Add("@agentId", requesMapped.clubId);
            parameters.Add("@clubCode", requesMapped.clubCode);
            parameters.Add("@filterDate", requesMapped.date);
            parameters.Add("@timeZoneOffsetValue", requesMapped.timeZoneOffsetValue);
            var dbResp = await _dao.ExecuteAsyncList<ScheduleResponseCommon>(proc, parameters);
            return dbResp;
        }

        public async Task<List<HostListResponseCommon>> HostsWithRankOrdering(HostListRequestCommon request)
        {
            string sp_name = "[dbo].[apiproc_cp_get_hosts_with_rank_ordering]";
            var param = new DynamicParameters();
            param.Add("@clubId", request.clubId);
            param.Add("@clubCode", request.clubCode);
            param.Add("@userId", request.userId);
            param.Add("@pageNo", request.pageNo);
            param.Add("@pageSize", request.pageSize);
            var dbResponse = await _dao.ExecuteAsyncList<HostListResponseCommon>(sp_name, param);
            return dbResponse;
        }

        public async Task<(HostDetailsV2Common? hostDetails, List<HostImagesV2Common> hostImages)>
            GetHostDetailsV2Async(long? hostId = null, long? hostCode = null, long? userId = null)
        {
            const string hostDetailsProc = "dbo.apiproc_cp_get_host_detail_v2";
            const string hostImagesProc = "dbo.apiproc_cp_get_host_images_v2";

            var parameters = new DynamicParameters();
            parameters.Add("@hostId", hostId);
            parameters.Add("@hostCode", hostCode);
            parameters.Add("@userId", userId);

            var hostDetailsResponse = await _dao.ExecuteAsync<HostDetailsV2Common>(
                hostDetailsProc,
                parameters
            );

            if (hostDetailsResponse == null)
            {
                return (null, new List<HostImagesV2Common>());
            }

            var imageParameters = new DynamicParameters();
            imageParameters.Add("@hostId", hostId);
            imageParameters.Add("@hostCode", hostCode);

            var hostImagesResponse = await _dao.ExecuteAsyncList<HostImagesV2Common>(
                hostImagesProc,
                imageParameters
            );

            return (hostDetailsResponse, hostImagesResponse?.ToList() ?? new List<HostImagesV2Common>());
        }
    }
}
