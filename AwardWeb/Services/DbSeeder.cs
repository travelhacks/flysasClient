using AwardData;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace AwardWeb
{
    public class DbSeeder 
    {
        public static void Seed(AwardData.AwardContext ctx, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            var routes = getRoutes();
            IEnumerable<Crawl> Crawls = null;
            ctx.Routes.AddRange(routes);
            ctx.SaveChanges();
            var client = new HttpClient();
            using (HttpResponseMessage response = client.GetAsync("https://awardhacks.se/export/flights").Result)
            {
                using (HttpContent content = response.Content)
                {
                    var json = content.ReadAsStringAsync().Result;
                    var export = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AwardExport>>(json);
                    foreach (var e in export)
                    {
                        var tmp = new Crawl
                        {
                            Id = e.Id,
                            Departure = e.Departure,
                            Arrival = e.Arrival,
                            Business = e.Business,
                            Plus = e.Plus,
                            Go = e.Go,
                            Flight = e.Flight,
                            TravelDate = e.Departure.Date,
                            CrawlDate = DateTime.Now,
                            Equipment = e.Equipment,
                            Success = true
                        };
                        tmp.RouteId = routes.Where(r => r.To == e.Destination && r.From == e.Origin).Select(r => r.Id).FirstOrDefault();
                        if (tmp.RouteId == 0)
                        {
                            tmp.RouteId = routes.Where(r => r.To == e.Origin && r.From == e.Destination).Select(r => r.Id).FirstOrDefault();
                            tmp.Return = true;
                        }
                        ctx.Crawls.Add(tmp);
                    }
                }
            }
            ctx.SaveChanges();
            var ofData = new OpenFlightsData.OFData();
            ofData.LoadData();
            int id = 0;
            foreach (var iata in ctx.Routes.Select(r => r.To).Union(ctx.Routes.Select(r => r.From)).Distinct())
            {
                var ap = ofData.Airports.FirstOrDefault(ap => iata.Equals(ap.IATA, StringComparison.OrdinalIgnoreCase));
                if (ap != null)
                    ctx.Airports.Add(new Airport

                    {
                        Id = ++id,
                        City = ap.City,
                        IATA = ap.IATA,
                        Country = ap.Country,
                        //This is not correct for Africa etc but will do for the current routes. 
                        Zone = ap.Country == "United States" ? "North & Central America" : ap.Timezone > -4 && ap.Timezone < 5 ? "Europe" : "Central Asia & Far East Asia"
                    }
                        ); ;

            }
            ctx.SaveChanges();            
            userManager.CreateAsync(new ApplicationUser { Email = "test@award.se", EmailConfirmed = true, UserName = "test@award.se" }, "someweaktestpwd");
            ctx.SaveChanges();
        }
        

        private static Route[] getRoutes()
        {
            return new Route[]
            {
                new Route { Crawl = true, Show = true, To = "NRT", From = "CPH", Id = 1 },
                new Route { Crawl = true, Show = true, To = "BOS", From = "CPH", Id = 2 },
                new Route { Crawl = true, Show = true, To = "ORD", From = "CPH", Id = 3 },
                new Route { Crawl = true, Show = true, To = "ORD", From = "ARN", Id = 4 },
                new Route { Crawl = true, Show = true, To = "LAX", From = "ARN", Id = 5 },
                new Route { Crawl = true, Show = true, To = "MIA", From = "OSL", Id = 6 },
                new Route { Crawl = true, Show = true, To = "MIA", From = "CPH", Id = 7 },
                new Route { Crawl = true, Show = true, To = "EWR", From = "ARN", Id = 8 },
                new Route { Crawl = true, Show = true, To = "IAD", From = "CPH", Id = 9 },
                new Route { Crawl = true, Show = true, To = "PEK", From = "CPH", Id = 11 },
                new Route { Crawl = true, Show = true, To = "PVG", From = "CPH", Id = 12 },
                new Route { Crawl = true, Show = true, To = "SFO", From = "CPH", Id = 14 },
                new Route { Crawl = true, Show = true, To = "EWR", From = "CPH", Id = 15 },
                new Route { Crawl = true, Show = true, To = "EWR", From = "OSL", Id = 16 },
                new Route { Crawl = true, Show = true, To = "MIA", From = "ARN", Id = 277 },
                new Route { Crawl = true, Show = true, To = "HKG", From = "CPH", Id = 279 },
                new Route { Crawl = true, Show = true, To = "LAX", From = "CPH", Id = 280 }
            };
        }

        public List<AwardData.Crawl> Crawls { get; private set; } = new List<Crawl>();

        public List<string> EquipmentList { get; private set; } = new List<string>();

        public void Set(List<Crawl> crawls)
        {
           
        }
    }
}
