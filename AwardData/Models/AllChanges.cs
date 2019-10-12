using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AwardData
{
    [Table("V_ALLChanges")]
    public class AllChanges 
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
        public int Plus { get; set; }
        public int Go { get; set; }
        public int? g2 { get; set; }
        public int? p2 { get; set; }

        public bool HasIncrease(BookingClass cabinClass)
        {
            return cabinClass switch
            {
                BookingClass.All => HasIncrease(BookingClass.Go)  || HasIncrease(BookingClass.Plus) || HasIncrease(BookingClass.Business),
                BookingClass.Business => Business > c2.GetValueOrDefault(),
                BookingClass.Plus => Plus > p2.GetValueOrDefault(),
                BookingClass.Go => Go > g2.GetValueOrDefault(),
                _ => false
            };
        }
            
    }
}
