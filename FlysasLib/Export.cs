using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace FlysasLib
{
    public class FlightExporter
    {

        public List<FlightExport> Convert(List<Transaction> transactions)
        {
            var OFData = new OpenFlightsData.OFData();
            OFData.LoadData();
            var exports = new List<FlightExport>();
            var map = new Dictionary<string, string>() { { "KF", "SK" } };
            foreach (var r in transactions)
                if (!r.Origin.IsNullOrEmpty())
                {                    
                    var o = OFData.Airports.Where(a => a.City.Equals(r.Origin, StringComparison.OrdinalIgnoreCase)).ToList();
                    var d = OFData.Airports.Where(a => a.City.Equals(r.Destination, StringComparison.OrdinalIgnoreCase)).ToList();
                    var tmp = OFData.Routes.Where(route => o.Any(a => a.IATA == route.FromIATA) && d.Any(a => a.IATA == route.ToIATA)).ToList();
                    var airline = OFData.Airlines.FirstOrDefault(a => a.IATA == r.Airline);
                    var airlineId = airline != null ? new int?(airline.ID) : new int?();
                    var matches = tmp.Where(route => route.AirlineCode == r.Airline || map.ContainsKey(route.AirlineCode) && map[route.AirlineCode] == r.Airline).ToList();
                    if (matches.Count == 0)
                    {                     
                        if (o.Count == 1 && d.Count == 1)                     
                            exports.Add(new FlightExport(r, o.First().IATA, d.First().IATA, airlineId, 50));                            
                        else
                            exports.Add(new FlightExport(r, r.Origin, r.Destination, airlineId, 0));
                    }
                    if (matches.Count > 1)
                        exports.Add(new FlightExport(r, r.Origin, r.Destination, airlineId, 0));
                    if (matches.Count == 1)
                        exports.Add(new FlightExport(r, matches.First().FromIATA, matches.First().ToIATA, airlineId, 99));
                }
            return exports;
        }

        public void SaveCSV(List<FlightExport> exportList)
        {            
            var folder = System.IO.Path.Combine(System.IO.Path.Combine(System.AppContext.BaseDirectory, "Export"));
            try
            {
                var current = GetCurrent(folder);
                exportList.RemoveAll(ex => current.Any(c => c.Flight == ex.Flight && c.Date == ex.Date));
            }
            catch
            {

            }
            var good = exportList.Where(f => f.Score >= 50).ToList();
            var bad = exportList.Where(f => f.Score < 50).ToList();
            if (good.Count > 0)
                save(Path.Combine(folder, "openflights_export" + DateTime.Now.ToString("yyyy-MM-dd")+ ".csv"), good);
            if (bad.Count > 0)
                save(Path.Combine(folder, "openflights_export_failed_flights" + DateTime.Now.ToString("yyyy-MM-dd")+".csv"), bad);            
        }

        void save(string filename, List<FlightExport> flights)
        {
            var header =  FlightExport.OpenFlightsHeader();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(new FileStream(filename, FileMode.Create)))
            {
                file.WriteLine(header);
                foreach (var row in flights)
                {
                    file.WriteLine(row.OpenFlightsString());
                }
            }
        }

        public List<FlightExport> GetCurrent(string folder)
        {
            var sDiary = System.IO.Path.Combine(folder, "flightdiary.csv");
            var sOpenFlights = System.IO.Path.Combine(folder, "openflights.csv");
            var res = new List<FlightExport>();
            if (File.Exists(sDiary))
            {
                foreach (var line in File.ReadAllText(sDiary).Split('\n').Skip(1))
                {
                    var cols = line.Split(',').ToList();
                    if (cols.Count > 1)
                    {
                        if (DateTime.TryParse(cols[0], out DateTime dt))
                        {
                            var x = new FlightExport
                            {
                                Date = dt,
                                Flight = cols[1].Trim('"')
                            };
                            res.Add(x);
                        }
                    }
                }
            }            
            if (File.Exists(sOpenFlights))
            {
                foreach (var line in File.ReadAllText(sOpenFlights).Split(new[] { "\r\n" },StringSplitOptions.None).Skip(1))
                {
                    var cols = line.Split(',').ToList();
                    if (cols.Count > 1)
                    {
                        if (DateTime.TryParse(cols[0], out DateTime dt))
                        {
                            var x = new FlightExport
                            {
                                Date = dt,
                                Flight = cols[3]
                            };
                            res.Add(x);
                        }
                    }
                }
            }
            return res;
        }
    }
    public class FlightExport
    {
        public DateTime Date { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Flight { get; set; }
        public int Score;
        public string CabinClass { get; set; }
        public int? AirlineId;
        public FlightExport() { }
        public FlightExport(Transaction t,string From, string To, int? airlineId, int score)
        {
            Date = t.datePerformed;
            this.From = From;
            this.To = To;
            this.Flight = t.Airline + t.FlightNumber;
            this.Score = score;
            this.CabinClass = t.CabinClass;
            this.AirlineId = airlineId;
        }
        public String OpenFlightsString()
        {
            //Class: one of "F" for First, "C" for Business, "P" for Premium Economy, "Y" for Economy.
            string c = "";
            if (CabinClass.Contains("SAS Go") || CabinClass.Contains("Economy"))
                c = "Y";
            if (CabinClass.Contains("SAS Plus"))
                c = "P";
            if (CabinClass.Contains("Business"))
                c = "C";
                return Date.ToString("yyyy-MM-dd") + $",{From},{To},{Flight},,,,,,{c},,,,,\"\",,,{AirlineId},";
        }
        public static string OpenFlightsHeader()
        {
            return "Date,From,To,Flight_Number,Airline,Distance,Duration,Seat,Seat_Type,Class,Reason,Plane,Registration,Trip,Note,From_OID,To_OID,Airline_OID,Plane_OID";
        }
    }

    public class FlightDiary
    {
        public DateTime Date { get; set; }
        public String FlightNumber { get; set; }
        public String DepTime { get; set; }
        public String ArrTime { get; set; }
        public String Duration { get; set; }
        public String Airline { get; set; }
        public String Aircraft { get; set; }
        public String SeatNumber { get; set; }
        public String SeatType { get; set; }
        public String FlightClass { get; set; }
        public String FlightReason { get; set; }
        public String Note { get; set; }
        public int DepId { get; set; }
        public int ArrId { get; set; }
        public int AirlineId { get; set; }
        public int AircraftId { get; set; }
    }
}