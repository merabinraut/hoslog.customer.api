using hoslog.customer.common.Models.Notice;
using hoslog.customer.shared.Notice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.domain.Notice
{
    public interface INoticeRepository
    {
        Task<List<NoticeDetailResponseCommon>> GetClubNoticeAsync(NoticeModel request);
    }
}
