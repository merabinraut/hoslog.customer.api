using Dapper;
using hoslog.customer.shared.PointWithDrawl;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace hoslog.customer.domain.PointWithdrawl
{
    public class PointWithdrawlRepository : IPointWithdrawlRepository
    {
        private readonly RepositoryDao _dao;

        public PointWithdrawlRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<RedeemPointGiftCardResultCommon> RedeemPointGiftCard(PointExchangeCommons model,long customerId)
        {
            string proc = "apiproc_customer_redeem_point_giftcard";
            var parameters = new DynamicParameters();
            parameters.Add("@customerId", customerId);
            parameters.Add("@pointAmount",model.pointAmount);
            parameters.Add("@actionUser", model.actionUserId);
            parameters.Add("@actionIP", model.actionIP);

            var dbResp = await _dao.ExecuteAsync<RedeemPointGiftCardResultCommon>(proc,parameters);
            return dbResp;
        }

    }
}
