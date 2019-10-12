using System;
using System.ComponentModel.DataAnnotations;

namespace AwardData
{
    public class Alerts
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [Display(Name = "Route")]
        public int RouteId { get; set; }
        public bool Return { get; set; }
        [Display(Name = "Earliest  (optional)")]
        public DateTime? FromDate { get; set; }
        [Display(Name = "Latest  (optional)")]
        public DateTime? ToDate { get; set; }
        public int Passengers { get; set; }
        [Display(Name = "Class")]
        public BookingClass CabinClass { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Route Route { get; set; }
        public DateTime Created { get; set; }
        public string Origin => Route.GetFrom(this.Return);

        public string Destination => Route.GetTo(this.Return);
        
        public bool IsActive
        {
            get
            {
                return ToDate == null || ToDate >= DateTime.Now.Date;
            }
        }

        public bool Matches(BookingClass bc, int pax)
        {
            return Passengers >= pax && (bc == CabinClass || CabinClass == BookingClass.All);                
        }

        public bool IsInRange(DateTime date)
        {
            var dt = date.Date;
            return (FromDate == null || FromDate <= dt) && (ToDate == null || ToDate >= dt);
        }
    }
}
