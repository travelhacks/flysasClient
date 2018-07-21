using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFlightsData
{
    public class Plane : OpenFlightDataClass
    {
        public string Name { get; private set; }
        public string IATA { get; private set; }
        public string ICAO { get; private set; }
        
        internal IReadOnlyList<Plane> GetAll()
        {
            var list = new List<Plane>();
            var fn = GetPath("planes.dat");
            if (System.IO.File.Exists(fn))
                foreach (var row in System.IO.File.ReadLines(fn))
                {
                    var cols = mySplit(row);
                    var a = new Plane
                    {
                        Name = cols[0],
                        IATA = cols[1],
                        ICAO = cols[2]
                    };
                    list.Add(a);
                }
            return list;
        }
    }
}
