using System.Collections.Generic;

namespace AwardData
{
    public class Route
    {
        public int Id { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public bool Crawl { get; set; }
        public bool Show { get; set; }
        public virtual List<Crawl> Crawls { get; set; }

        public string ToString(bool Return)
        {
            return GetFrom(Return) + "-" + GetTo(Return);
        }
        public string GetTo(bool Return)
        {
            return Return ? From : To;
        }
        public string GetFrom(bool Return)
        {
            return Return ? To : From;
        }
        public virtual Airport ToAirport { get; set; }
        public virtual Airport FromAirport { get; set; }
       
    }
}
