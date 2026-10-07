namespace hoslog.customer.common.Models.Host;

public class HostDetailsV2ResponseModel
{
    public StoreV2Model store { get; set; } = new();
    public HostV2Model host { get; set; } = new();
    // public BookingV2Model booking { get; set; } = new();
}

public class StoreV2Model
{
    public string? id { get; set; }
    public string? name { get; set; }
    public string? nameKana { get; set; }
    public string? area { get; set; }
    public string? logoUrl { get; set; }
}

public class HostV2Model
{
    public string? id { get; set; }
    public int rank { get; set; }
    public string? name { get; set; }
    public string? nameRoman { get; set; }
    public string? role { get; set; }
    public string? badge { get; set; }
    public bool isHostBookmarked { get; set; } = false;
    public HostStatsV2Model stats { get; set; } = new();
    public HostSocialLinksV2Model socialLinks { get; set; } = new();
    public HostProfileV2Model profile { get; set; } = new();
    public List<HostPhotoV2Model> photos { get; set; } = new();
    public HostingStyleV2Model hostingStyle { get; set; } = new();
}

public class HostStatsV2Model
{
    public int likes { get; set; }
    public int reviews { get; set; }
}

public class HostSocialLinksV2Model
{
    public string? instagram { get; set; }
    public string? tiktok { get; set; }
    public string? x { get; set; }
    public string? line { get; set; }
    public string? shareUrl { get; set; }
}

public class HostProfileV2Model
{
    public string? birthday { get; set; }
    public string heightCm { get; set; } = "0";
    public string? zodiac { get; set; }
    public string? birthplace { get; set; }
    public string? bloodType { get; set; }
    public string? mbti { get; set; }
}

public class HostPhotoV2Model
{
    public string? id { get; set; }
    public string? url { get; set; }
}

public class HostingStyleV2Model
{
    public int appearance { get; set; }
    public int conversationStyle { get; set; }
    public int alcoholTolerance { get; set; }
    public int atmosphere { get; set; }
}

// public class BookingV2Model
// {
//     public bool onlineBookingAvailable { get; set; }
//     public int onlineBookingRemainingSlots { get; set; }
//     public bool phoneBookingAvailable { get; set; }
//     public int pointsPerPerson { get; set; }
// }