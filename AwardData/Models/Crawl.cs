using System;

namespace AwardData
{
    public class Crawl 
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public bool Return { get; set; }
        public DateTime TravelDate { get; set; }
        public DateTime CrawlDate { get; set; }
        public DateTimeOffset? Arrival { get; set; }
        public DateTimeOffset? Departure { get; set; }
        public string Flight { get; set; }
        public virtual Route Route { get; set; }
        public int Go { get; set; }
        public int Plus { get; set; }
        public int Business { get; set; }
        public bool Success { get; set; }
        public string Equipment { get; set; }
        public int First { get; set; }
        public int? ExternalId { get; set; }

        public int? ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string Destination
        {
            get
            {
                return Route.GetTo(Return);
            }
        }
        public string Origin
        {
            get
            {
                return Route.GetFrom(Return);
            }
        }
        public string TimeString
        {
            get
            {
                var frmt = "HH:mm";
                var s = Departure.Value.ToString(frmt) + "-" + Arrival.Value.ToString(frmt);
                var diff = (int)Arrival.Value.Date.Subtract(Departure.Value.Date).TotalDays;
                if (diff != 0)
                    s += (diff > 0 ? "+" : "-") + diff.ToString();
                return s;
            }
        }

        public string AirLine { get { return Flight.Substring(0, 2); } }

        public bool HasSpace(int cabinClass, uint passengers)
        {
            int space = cabinClass switch
            {
                (int)BookingClass.Business => this.Business,
                (int)BookingClass.Plus => this.Plus,
                (int)BookingClass.Go => this.Go,
                _ => Math.Max(Go, Math.Max(Business, Plus))
            };
            return space >= passengers;
        }
    }
}
