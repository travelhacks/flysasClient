using AwardData;
using System.Collections.Generic;

namespace AwardWeb.Models
{
    public class SearchResult
    {
        public Crawl Out { get; set; }
        public Crawl In { get; set; }
    }
    public class ResultContainer
    {
        public List<SearchResult> Flights { get; set; }
        public uint Pax { get; set; }
        public bool Return { get; set; }
        public int iClass { get; set; }

        public int RowLimit { get; set; }
        public string Site { get; set; }
    }
}
