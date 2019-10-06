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
        public string Origin
        {
            get
            {
                return Route != null ?  Route.GetFrom(this.Return) : "";
            }
        }
        public string Destination
        {
            get
            {
                return Route != null ?  Route.GetTo(this.Return) :"";
            }
        }
        public bool IsActive =>  (!ToDate.HasValue || ToDate.Value >= DateTime.Now.Date);
        
        
    }
}
