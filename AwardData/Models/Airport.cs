using System.Collections.Generic;

namespace AwardData
{
    public class Airport
    {
        public int Id { get; set; }
        public string IATA { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Zone { get; set; }
        public string City { get; set; }
        public virtual List<Route> RoutesFrom { get; set; }
        public virtual List<Route> RoutesTo { get; set; }
    }
}
