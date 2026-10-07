using System;
using System.Collections.Generic;
using System.Text;

namespace hoslog.customer.common.Models.PointWithdrawl
{

    public class RedeemPointGiftCardModel
    {
        public decimal pointAmount { get; set; }
    }

    public class RedeemPointGiftResponseModel
    {
        public string? WithdrawalId { get; set; }
        public string? WithdrawalRequestId { get; set; }
        public decimal? BalanceBefore { get; set; }
        public decimal? BalanceAfter { get; set; }
        public decimal? PayoutAmount { get; set; }
    }
}
