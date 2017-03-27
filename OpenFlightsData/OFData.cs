using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFlightsData
{
    public class OFData
    {
        public IReadOnlyList<Airline> Airlines { get; private set; } = new List<Airline>();
        public IReadOnlyList<Airport> Airports { get; private set; } = new List<Airport>();
        public IReadOnlyList<Route> Routes { get; private set; } = new List<Route>();

        public void LoadData()
        {
            Airlines = new Airline().GetAll();
            Airports =  new Airport().GetAll();
            Routes = new Route().GetAll();
        }
    }
    
}
