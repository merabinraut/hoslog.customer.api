using hoslog.customer.shared.PointWithDrawl;
using System;
using System.Collections.Generic;
using System.Text;

namespace hoslog.customer.domain.PointWithdrawl
{
    public interface IPointWithdrawlRepository
    {
       Task<RedeemPointGiftCardResultCommon> RedeemPointGiftCard(PointExchangeCommons model, long customerId);
    }
}
