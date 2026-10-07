using hoslog.customer.common.Models.Notice;
using hoslog.customer.shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.business.Notice
{
    public interface INoticeBusiness
    {
        Task<(HttpStatusCode, object)> GetClubNoticeAsync(int pageNo,string clubId,string clubCode);
    }
}
