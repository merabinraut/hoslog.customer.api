namespace hoslog.customer.common.Models.ClubManagement
{
    public class ClubScheduleModel
    {
        public long date { get; set; }
        public string englishDay { get; set; } = null!;
        public string japaneseDay { get; set; } = null!;
        public string scheduleLabel { get; set; } = null!;
        public string schedule { get; set; } = null!;
    }
}