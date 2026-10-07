using hoslog.customer.business.ApiLog;
using hoslog.customer.business.Banner;
using hoslog.customer.business.ExceptionLog;
using hoslog.customer.domain.ApiLog;
using hoslog.customer.domain.Banner;
using hoslog.customer.business.User;
using hoslog.customer.domain.User;
using hoslog.customer.domain.Auth;
using hoslog.customer.business.Auth;
using hoslog.customer.domain.ExceptionLog;
using hoslog.customer.business.Advertisement;
using hoslog.customer.domain.Advertisement;
using hoslog.customer.business.UserRegistration;
using hoslog.customer.domain.UserRegistration;
using hoslog.customer.business.LocationManagement;
using hoslog.customer.domain.LocationManagement;
using hoslog.customer.business.Plans;
using hoslog.customer.domain.Plans;
using hoslog.customer.business.CommonAttribute;
using hoslog.customer.domain.CommonAttribute;
using hoslog.customer.business.Review;
using hoslog.customer.domain.Review;
using hoslog.customer.domain.Profile;
using hoslog.customer.business.Profile;
using hoslog.customer.business.Reservation;
using hoslog.customer.domain.Reservation;
using hoslog.customer.business.Inquiries;
using hoslog.customer.domain.Inquiries;
using hoslog.customer.business.Host;
using hoslog.customer.domain.Host;
using hoslog.customer.business.ClubManagement;
using hoslog.customer.domain.ClubManagement;
using hoslog.customer.business.Notice;
using hoslog.customer.domain.Notice;
using hoslog.customer.business.Recommendation;
using hoslog.customer.domain.Recommendation;
using hoslog.customer.business.Bookmark;
using hoslog.customer.domain.Bookmark;
using hoslog.customer.business.Notification;
using hoslog.customer.domain.Notification;
using hoslog.customer.business.Search;
using hoslog.customer.domain.Search;
using hoslog.customer.business.PointManagement;
using hoslog.customer.domain.PointManagement;
using hoslog.customer.business.Group;
using System.Text.RegularExpressions;
using hoslog.customer.domain.Group;
using hoslog.customer.business.ApiMessageManagement;
using hoslog.customer.business.Conversion;
using hoslog.customer.domain.ApiMessageManagement;
using hoslog.customer.business.Multitenant;
using hoslog.customer.domain.Conversion;
using hoslog.customer.domain.Multitenant;
using hoslog.customer.domain.SSE;
using hoslog.customer.business.ReferralManagement;
using hoslog.customer.domain.ReferralManagement;
using hoslog.customer.business.PointWithdrawl;
using hoslog.customer.domain.PointWithdrawl;

namespace hoslog.customer.api.Services
{
    public static class Business
    {
        public static void RegisterBusiness(this IServiceCollection services)
        {
            services.AddScoped<IBannerBusiness, BannerBusiness>();
            services.AddScoped<IAdvertisementBusiness, AdvertisementBusiness>();
            services.AddScoped<IApiLogBusiness, ApiLogBusiness>();
            services.AddScoped<IExceptionLogBusiness, ExceptionLogBusiness>();
            services.AddScoped<IUserBusiness, UserBusiness>();
            services.AddScoped<IAuthBusiness, AuthBusiness>();
            services.AddScoped<ILocationManagementBusiness, LocationManagementBusiness>();
            services.AddScoped<IPlanBusiness, PlanBusiness>();
            services.AddScoped<IUserRegistrationBusiness, UserRegistrationBusiness>();
            services.AddScoped<ICommonAttributeBusiness, CommonAttributeBusiness>();
            services.AddScoped<IReviewBusiness, ReviewBusiness>();
            services.AddScoped<IProfileBusiness, ProfileBusiness>();
            services.AddScoped<IReservationHistoryBusiness, ReservationHistoryBusiness>();
            services.AddScoped<IInquiriesBusiness, InquiriesBusiness>();
            services.AddScoped<IHostBusiness, HostBusiness>();
            services.AddScoped<IClubManagementBusiness, ClubManagementBusiness>();
            services.AddScoped<INoticeBusiness, NoticeBusiness>();
            services.AddScoped<IRecommendationBusiness, RecommendationBusiness>();
            services.AddScoped<IBookmarkBusiness, BookmarkBusiness>();
            services.AddScoped<IReservationBusiness, ReservationBusiness>();
            services.AddScoped<INotificationBusiness, NotificationBusiness>();
            services.AddScoped<ISearchBusiness, SearchBusiness>();
            services.AddScoped<IPointManagementBusiness, PointManagementBusiness>();
            services.AddScoped<IGroupBusiness, GroupBusiness>();
            services.AddScoped<IApiMessageManagementBusiness, ApiMessageManagementBusiness>();
            services.AddScoped<IClubInformationMultitenantBusiness, ClubInformationMultitenantBusiness>();
            services.AddScoped<IReservationMultitenantBusiness, ReservationMultitenantBusiness>();
            services.AddScoped<IConversionBusiness,ConversionBusiness>();
            services.AddScoped<IReferralPointBusiness, ReferralPointBusiness>();
            services.AddScoped<IPointWithdrawlBusiness, PointWithdrawlBusiness>();
        }
    }
    public static class Domain
    {
        public static void RegisterDomain(this IServiceCollection services)
        {
            services.AddScoped<IBannerRepository, BannerRepository>();
            services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
            services.AddScoped<IApiLogRepository, ApiLogRepository>();
            services.AddScoped<IExceptionLogRepository, ExceptionLogRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<ILocationManagementRepository, LocationManagementRepository>();
            services.AddScoped<IPlanRepository, PlanRepository>();
            services.AddScoped<IUserRegistrationRepository, UserRegistrationRepository>();
            services.AddScoped<ICommonAttributeRepository, CommonAttributeRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IInquiriesRepository, InquiriesRepository>();
            services.AddScoped<IHostRepository, HostRepository>();
            services.AddScoped<IReservationHistoryRepository, ReservationHistoryRepository>();
            services.AddScoped<IInquiriesRepository, InquiriesRepository>();
            services.AddScoped<IHostRepository, HostRepository>();
            services.AddScoped<IClubManagementRepository, ClubManagementRepository>();
            services.AddScoped<INoticeRepository, NoticeRepository>();
            services.AddScoped<IRecommendationRepository, RecommendationRepository>();
            services.AddScoped<IBookmarkRepository, BookmarkRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ISearchRepository, SearchRepository>();
            services.AddScoped<IPointManagementRepository, PointManagementRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IApiMessageManagementRepository, ApiMessageManagementRepository>();
            services.AddScoped<IClubInformationMultitenantRepository, ClubInformationMultitenantRepository>();
            services.AddScoped<IReservationMultitenantRepository, ReservationMultitenantRepository>();
            services.AddScoped<IVisitNowRepository, VisitNowRepository>();
            services.AddScoped<IConversionRepository, ConversionRepository>();
            services.AddScoped<IReferralPointRepository, ReferralPointRepository>();
            services.AddScoped<IPointWithdrawlRepository, PointWithdrawlRepository>();
        }
    }
}
