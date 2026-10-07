using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.common.Models.Notice
{
    public class NoticeModel:PaginationRequestModel
    {
      public string? clubId {  get; set; }
      public string? clubCode {  get; set; }
    }
}
