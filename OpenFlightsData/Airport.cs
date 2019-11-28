using System;
using System.Collections.Generic;

namespace OpenFlightsData
{
    public class Airport : OpenFlightDataClass
    {
        public int ID { get; set; }
        public String Name { get; set; }
        public String City { get; set; }
        public String Country { get; set; }
        public String IATA { get; set; }
        public string ICAO { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public double Alt { get; set; }
        public double Timezone { get; set; }
        public string DST { get; set; }
        public string TZ { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }

        public List<Airport> GetAll()
        {
            var list = new List<Airport>();
            var fn = GetPath("airports.dat");
            if (System.IO.File.Exists(fn))
                foreach (var row in System.IO.File.ReadLines(fn))
                {
                    var cols = mySplit(row);
                    var a = new Airport
                    {
                        ID = int.Parse(cols[0]),
                        Name = cols[1],
                        City = cols[2],
                        Country = cols[3],
                        IATA = cols[4],
                        ICAO = cols[5],
                        Lat = myDouble(cols[6]),
                        Long = myDouble(cols[7]),
                        Alt = myDouble(cols[8]),
                        Timezone = myDouble(cols[9]),
                        DST = cols[10],
                        Type = cols[11],
                        Source = cols[12]

                    };
                    list.Add(a);
                }
            return list;
        }
    }
}
