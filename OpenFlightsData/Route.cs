using System.Collections.Generic;

namespace OpenFlightsData
{
    public class Route : OpenFlightDataClass
    {
        public string AirlineCode { get; set; }
        public string AirlineId { get; set; }
        public string FromIATA { get; set; }
        public int? FromId { get; set; }
        public string ToIATA { get; set; }
        public int? ToId { get; set; }
        public bool CodeShare { get; set; }
        public int? Stops { get; set; }
        public string Equipment { get; set; }


        public List<Route> GetAll()
        {
            var list = new List<Route>();
            var enc = System.Text.Encoding.GetEncoding("iso-8859-1");
            var fn = GetPath("routes.dat");
            if (System.IO.File.Exists(fn))
                foreach (var row in System.IO.File.ReadLines(fn, enc))
                {
                    var cols = mySplit(row);
                    var a = new Route
                    {
                        AirlineCode = cols[0],
                        AirlineId = cols[1],
                        FromIATA = cols[2],
                        FromId = myInt(cols[3]),
                        ToIATA = cols[4],
                        ToId = myInt(cols[5]),
                        CodeShare = cols[6] == "Y",
                        Stops = myInt(cols[7]),
                        Equipment = cols[8],
                    };
                    list.Add(a);
                }
            return list;
        }
    }
}
