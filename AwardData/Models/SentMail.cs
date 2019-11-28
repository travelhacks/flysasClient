using System;

namespace AwardData
{
    public class SentMail
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CrawlId { get; set; }
        public DateTime Date { get; set; }
        public virtual Crawl Crawl { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
