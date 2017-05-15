using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;


namespace AwardData
{
    public class AwardCrawler
    {
        AwardContext ctx;
        public AwardCrawler(AwardContext context)
        {
            ctx = context;
        }

        public void Crawl(int start,int stop,bool force,bool checkOldEmpty,bool checkExisting, int MaxAgeInDays,int sleepSeconds)
        {
            var sasClient = new FlysasLib.SASRestClient();
                               
            foreach(var route in ctx.Routes.ToList())
            {                
                foreach (var returnFlight in new[] { false, true })
                {
                    var oldFlights = ctx.Crawls.FromSql("select * from v_latestcrawls").Where(crawl => crawl.RouteId == route.Id && crawl.Return == returnFlight && crawl.Success).ToList();
                    for (int i = start; i <= stop; i++)
                    {
                        var q = new FlysasLib.SASQuery
                        {
                            OutDate = DateTime.Now.Date.AddDays(i),
                            From = returnFlight ? route.To : route.From,
                            To = returnFlight ? route.From : route.To,
                        };
                        try
                        {                            
                            var old = oldFlights.Where(crawl => crawl.RouteId == route.Id && crawl.Return == returnFlight && crawl.TravelDate == q.OutDate.Value && crawl.Success).OrderByDescending(f => f.CrawlDate).FirstOrDefault();
                            if (old != null)
                            {
                                if (!force && (!checkExisting && old.Business > 0 || old.Business == 0 && (!checkOldEmpty || old.CrawlDate > DateTime.UtcNow.AddDays(-MaxAgeInDays))))
                                    continue;
                            }

                            FlysasLib.AwardResult res = null;
                            var availability = new int[3];
                            FlysasLib.AwardFlight flight = null;
                            do
                            {
                                res = sasClient.Award(q);
                                System.Threading.Thread.Sleep(1000 * sleepSeconds);
                                if (res != null)
                                {
                                    if (res != null && res.outbounds != null)
                                    {
                                        foreach (var f in res.outbounds.Where(f => f.segments.Count == 1))
                                        {
                                            if (f.products.BUSINESS != null)
                                                availability[2] = q.Adt;
                                            if (f.products.PLUS != null)
                                                availability[1] = q.Adt;
                                            if (f.products.GO != null)
                                                availability[0] = q.Adt;
                                            flight = f;
                                        }

                                    }
                                }
                                q.Adt++;
                            } while (res != null && res.httpSuccess && res.outbounds.Any(f => f.products != null && (f.products.BUSINESS != null)));

                            var c = new AwardData.Crawl { Business = availability[2], Plus = availability[1], Go = availability[0], CrawlDate = DateTime.UtcNow, Return = returnFlight, RouteId = route.Id, TravelDate = q.OutDate.Value.Date };
                            if (flight != null)
                            {
                                c.Departure = flight.segments[0].startTimeLocal;
                                c.Arrival = flight.segments[0].endTimeInLocal;
                                c.Flight = flight.segments[0].flightNumber;
                            }
                            c.Success = res != null && (res.errors == null || res.errors.First().errorCode == "225036");
                            ctx.Crawls.Add(c);
                            ctx.SaveChanges();
                            if (availability.Any(j => j > 0))
                                Console.WriteLine(route.From + "-" + route.To + " " + q.OutDate.Value.ToString("yyyyMMdd") + ":" + string.Join(",", availability));
                        }

                        catch (Exception ex)
                        {
                            Console.Write("Error for " + q.OutDate.ToString());
                        }
                    }
                }
            }
            Console.ReadKey();
        }
    }
}
