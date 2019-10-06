using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace AwardWeb.Controllers
{

    public class ExportController : Controller
    {
        private readonly Services.ICachedData data;

        public ExportController(Services.ICachedData data)
        {
            this.data = data;
        }
        [HttpGet()]
        public IActionResult Flights()
        {
            var export = data.Crawls.Where(c => c.Route.Show).Select
            (c =>
                    new AwardExport
                    {
                        Id = c.Id,
                        Flight = c.Flight,
                        Arrival = c.Arrival.Value,
                        Departure = c.Departure.Value,
                        Business = c.Business,
                        Go = c.Plus,
                        Plus = c.Plus,
                        Origin = c.Route.GetFrom(c.Return),
                        Destination = c.Route.GetTo(c.Return),
                        Updated = new DateTime(c.CrawlDate.Ticks, DateTimeKind.Utc),
                        Equipment = c.Equipment
                    }
            ).ToArray();
            return Ok(export);
        }
    }
}