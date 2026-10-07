using System;
using System.Collections.Generic;
using System.Text;

namespace hoslog.customer.shared.PointWithDrawl
{

    public class PointExchangeCommon
    {
        public decimal ExchangeableBalance { get; set; }
        public decimal MinExchangeAmount { get; set; }
        public decimal ExchangeIncrement { get; set; }
        public List<PointExchangeOptionCommon> Options { get; set; } = new List<PointExchangeOptionCommon>();
    }

    public class PointExchangeCommons : CommonDBRequest
    {
        public string pointAmount { get; set; }
        public string currencyCode { get; set; }
    }

    public class PointExchangeOptionCommon
    {
        public int OptionId { get; set; }
        public decimal PointAmount { get; set; }
        public bool IsSelectable { get; set; }
        public decimal BalanceAfterIfSelected { get; set; }
    }

    public class RedeemPointGiftCardResultCommon : CommonDBResponse
    {
        public string? WithdrawalId { get; set; }
        public string? WithdrawalRequestId { get; set; }
        public decimal? BalanceBefore { get; set; }
        public decimal? BalanceAfter { get; set; }
        public decimal? PayoutAmount { get; set; }
    }
}
