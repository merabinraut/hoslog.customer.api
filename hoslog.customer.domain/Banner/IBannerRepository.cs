using hoslog.customer.shared.Banner;

namespace hoslog.customer.domain.Banner
{
    public interface IBannerRepository
    {
        Task<List<BannerCommon>> GetBannerList();
    }
}
