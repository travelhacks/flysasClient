using AwardData;

namespace AwardWeb.Models
{
    public class MailContainer
    {
        public AllChanges Crawl { get; set; }
        public uint Pax { get; set; }
        public CabinClass CabinClass { get; set; }
    }
}