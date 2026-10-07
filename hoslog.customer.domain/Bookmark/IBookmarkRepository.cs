using hoslog.customer.common.Models.BookmarkModel;
using hoslog.customer.shared;

namespace hoslog.customer.domain.Bookmark;

public interface IBookmarkRepository
{
    Task<IList<BookmarkedClubModel>> GetBookmarkedClubList(string? userId, string timeZoneOffsetValue = "");
    Task<IList<BookmarkedHostModel>> GetBookmarkedHostList(string? userId);
    Task<CommonDBResponse> ManageBookmark(BookmarkCommonModel dbRequest);
}
