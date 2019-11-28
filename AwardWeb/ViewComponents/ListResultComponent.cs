using AwardData;
using AwardWeb.Models;
using AwardWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace AwardWeb
{
    public class ListResultViewComponent : ViewComponent
    {
        ICachedData data;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager;

        public ListResultViewComponent(ICachedData cachedData, [FromServices] Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            data = cachedData;
            this.userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(SASSearch search)
        {
            List<SearchResult> result = new List<SearchResult>();
            int limit = 200;

            IEnumerable<Crawl> inBound = data.Crawls;
            IEnumerable<Crawl> outBound = data.Crawls.Where(c => crawlFilter(search.From, search.To, c));
            outBound = Filter(outBound, search.OutMin, search.OutMax, search.Passengers, search.CabinClass, search.OutWeekDays, search.Equipment);
            if (search.Return && search.To != null && search.To.Any(s => s != "All"))
                inBound = inBound.Where(c => crawlFilter(search.To, search.From, c));
            if (search.Return)
            {
                if (search.To != null && search.To.Any(s => s != "All"))
                    inBound = inBound.Where(c => crawlFilter(search.To, search.From, c));
                inBound = Filter(inBound, search.InMin, search.InMax, search.Passengers, search.CabinClass, search.InWeekDays, search.Equipment);
                result = GetResults(outBound, inBound, search.MinDays, search.MaxDays, limit, search.OpenJaw);
            }
            else
                result = outBound.OrderBy(f => f.Departure).Take(limit).Select(c => new SearchResult { Out = c }).ToList();

            string site = "";
            if (User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(HttpContext.User);
                site = user.Site;
            }

            var viewResult = View(new ResultContainer { Site = site, Flights = result, iClass = search.CabinClass, Pax = search.Passengers, Return = search.Return, RowLimit = limit });
            return viewResult;
        }

        bool crawlFilter(List<string> FromList, List<string> ToList, Crawl c)
        {
            return
                c.Departure >= DateTime.Now
                &&
               (FromList.Contains("All")
               || FromList.Contains(c.Return ? c.Route.To : c.Route.From)
               || FromList.Contains(c.Return ? c.Route.ToAirport.Zone : c.Route.FromAirport.Zone))
               &&
               (ToList.Contains("All")
               || ToList.Contains(c.Return ? c.Route.From : c.Route.To)
               || ToList.Contains(c.Return ? c.Route.FromAirport.Zone : c.Route.ToAirport.Zone));

        }



        private List<SearchResult> GetResults(IEnumerable<Crawl> outBound, IEnumerable<Crawl> inBound, uint minDays, uint maxDays, int limit, bool openJaw)
        {
            var scandi = new HashSet<string>(new[] { "CPH", "ARN", "OSL" });
            var res = new List<SearchResult>();
            var hash = inBound.GroupBy(c => c.Origin).ToDictionary(g => g.Key, g => g.ToList());
            hash["Scandinavia"] = inBound.Where(c => scandi.Contains(c.Origin)).ToList();

            foreach (var ob in outBound.OrderBy(f => f.Departure.Value))
            {
                var key = openJaw && !scandi.Contains(ob.Origin) ? "Scandinavia" : ob.Destination;
                if (hash.ContainsKey(key))
                    foreach (var ib in hash[key].OrderBy(f => f.Departure.Value))
                    {
                        var diff = ib.Departure.Value.Subtract(ob.Arrival.Value);
                        if (ob.Origin == ib.Destination || openJaw
                            && (scandi.Contains(ob.Origin) && scandi.Contains(ib.Destination)))
                            if (diff.TotalDays >= minDays && diff.TotalDays <= maxDays)
                            {
                                res.Add(new SearchResult { Out = ob, In = ib });
                                if (res.Count == limit)
                                    return res;
                            }
                    }
            }
            return res;
        }
        private IEnumerable<Crawl> Filter(IEnumerable<Crawl> collection, DateTime? minDate, DateTime? maxDate, uint passengers, int cabinClass, List<int> WeekDays, string equipment)
        {
            return collection.Where(c =>
            c.HasSpace(cabinClass, passengers) &&
            (!minDate.HasValue || minDate <= c.TravelDate) &&
            (!maxDate.HasValue || maxDate >= c.TravelDate) &&
            (WeekDays == null || !WeekDays.Any() || WeekDays.Contains(FlysasLib.ExtensionMethods.MyDayOfWeek(c.TravelDate)))
            && (string.IsNullOrEmpty(equipment) || c.Equipment == equipment)
            );
        }

    }
}
