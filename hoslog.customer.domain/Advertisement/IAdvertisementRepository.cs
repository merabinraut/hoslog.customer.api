using hoslog.customer.shared.Advertisement;
using hoslog.customer.shared.Banner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.domain.Advertisement
{
    public interface IAdvertisementRepository
    {
        Task<List<AdvertisementCommon>> GetAdvertisementList();
    }
}
