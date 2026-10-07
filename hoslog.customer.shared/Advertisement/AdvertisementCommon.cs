using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.shared.Advertisement
{
    public class AdvertisementCommon
    {
        public string? sno { get; set; }
        public string? advertisementTitle { get; set; }
        public string? advertisementImage { get; set; }
        public string? advertisementDescription { get; set; }
        public string? link { get; set; }
        public int displayOrder { get; set; }
    }

    public class AdvertisementResponse
    {
        public string? id { get; set; }
        public string? title { get; set; }
        public string? link { get; set; }
        public ImageResponse? image { get; set; }
        public int displayOrder { get; set; }
    }
}
