using System;

namespace AwardWeb
{
    public class AwardExport
    {
        public int Id { get; set; }
        public string Flight { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime Updated { get; set; }
        public DateTimeOffset Arrival { get; set; }
        public DateTimeOffset Departure { get; set; }
        public string Equipment { get; set; }

        public int Business { get; set; }
        public int Plus { get; set; }
        public int Go { get; set; }
    }
}
