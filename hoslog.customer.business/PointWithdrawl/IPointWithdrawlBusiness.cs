using hoslog.customer.common.Models.PointWithdrawl;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace hoslog.customer.business.PointWithdrawl
{
    public interface IPointWithdrawlBusiness
    {

        Task<(HttpStatusCode, object)> RedeemPointGiftCard(RedeemPointGiftCardModel model);

    }
}
