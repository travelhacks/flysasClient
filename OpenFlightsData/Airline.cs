using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFlightsData
{
    public class Airline : OpenFlightDataClass
    {
        public int ID { get; set; }
        public String Name { get; set; }
        public String Alias { get; set; }
        public String IATA { get; set; }
        public String ICAO { get; set; }
        public string Callsign { get; set; }
        public string Country { get; set; }
        public bool Active { get; set; }


        public List<Airline> GetAll()
        {
            var list = new List<Airline>();
            var enc = System.Text.Encoding.GetEncoding("iso-8859-1");
            var fn = GetPath("airlines.dat");
            if(System.IO.File.Exists(fn))
            foreach (var row in System.IO.File.ReadLines(fn, enc))
            {
                    var cols = mySplit(row);
                    var a = new Airline
                {
                    ID = int.Parse(cols[0]),
                    Name = cols[1],
                    Alias = cols[2],
                    IATA = cols[3],
                    ICAO = cols[4],
                    Callsign = cols[5],
                    Country = cols[6],
                    Active = cols[7] == "Y"
                };
                list.Add(a);
            }
            return list;
        }
    }
}
