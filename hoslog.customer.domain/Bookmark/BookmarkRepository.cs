using Dapper;
using hoslog.customer.common.Models.BookmarkModel;
using hoslog.customer.common.Models.RecommendationModel;
using hoslog.customer.shared;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Bookmark;

public class BookmarkRepository : IBookmarkRepository
{
    private readonly RepositoryDao _dao;
    public BookmarkRepository(IConfiguration configuration)
    {
        _dao = new RepositoryDao(configuration);
    }

    public async Task<IList<BookmarkedClubModel>> GetBookmarkedClubList(string? userId, string timeZoneOffsetValue = "")
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_get_bookmarked_club_list]";
        param.Add("@userId", userId);
        param.Add("@timeZoneOffsetValue", timeZoneOffsetValue);
        var dbResponse = await _dao.ExecuteAsyncList<BookmarkedClubModel>(sp_name, param);
        foreach (var item in dbResponse)
        {
            #region REVIEW AND RATING VIA CLUBID
            string sp_name_rnr = "[dbo].[apiproc_cp_club_review_and_rating_detail]";
            var param_rnr = new DynamicParameters();
            param_rnr.Add("@clubId", item.clubId);
            var dbRnrResponse = await _dao.ExecuteAsyncList<BookmarkedClubModel>(sp_name_rnr, param_rnr);
            if (dbRnrResponse != null)
            {
                item.averageRating = Convert.ToInt32(dbRnrResponse[0].averageRating);
                item.totalComment = Convert.ToInt32(dbRnrResponse[0].totalComment);
            }
            #endregion

            #region CLUB HOST IMAGE 
            List<HostImageForClubCard> hostImage = new List<HostImageForClubCard>();
            string sp_name_hi = "[dbo].[apiproc_cp_get_host_for_club]";
            var host_param = new DynamicParameters();
            host_param.Add("@clubId", item.clubId);
            var dbHostImageReponse = await _dao.ExecuteAsyncList<HostImageForClubCard>(sp_name_hi, host_param);
            foreach (var hostImage_item in dbHostImageReponse)
            {
                hostImage.Add(hostImage_item);
            }
            item.clubHostImages = hostImage;
            #endregion
        }
        return dbResponse;
    }
    public async Task<IList<BookmarkedHostModel>> GetBookmarkedHostList(string? userId)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_get_bookmarked_host_list]";
        param.Add("@userId", userId);
        var dbResponse = await _dao.ExecuteAsyncList<BookmarkedHostModel>(sp_name, param);
        return dbResponse;
    }

    public async Task<CommonDBResponse> ManageBookmark(BookmarkCommonModel dbRequest)
    {
        var param = new DynamicParameters();
        string sp_name = "[dbo].[apiproc_cp_manage_bookmark]";
        param.Add("@agentId", dbRequest.agentId);
        param.Add("@hostId", dbRequest.hostId);
        param.Add("@userId", dbRequest.userId);
        param.Add("@type", dbRequest.type);
        param.Add("@actionPlatform", dbRequest.actionPlatform);
        var dbResponse = await _dao.ExecuteAsync<CommonDBResponse>(sp_name, param);
        return dbResponse;
    }
}
