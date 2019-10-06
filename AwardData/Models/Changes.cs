using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AwardData
{
    [Table("V_Changes")]
    public class Changes
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public bool Return { get; set; }
        public DateTime TravelDate { get; set; }
        public DateTime CrawlDate { get; set; }
        public virtual Route Route { get; set; }        
        public int Business { get; set; }
        public String Flight { get; set; }
        public int? c2 { get; set; }
    }
}
