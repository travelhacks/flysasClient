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
            var origins =  normalizeSearch(search.From);
            var destinations = normalizeSearch(search.To);
            IEnumerable<Crawl> inBound = data.Crawls;
            IEnumerable<Crawl> outBound = data.Crawls.Where(c => routeFilter(origins, destinations, c));
            

            outBound = Filter(outBound, search.OutMin, search.OutMax, search.Passengers, search.CabinClass, search.OutWeekDays,search.Equipment);            
            if (search.Return)
            {                
                inBound = inBound.Where(c => routeFilter(destinations, origins, c));
                inBound = Filter(inBound, search.InMin, search.InMax, search.Passengers, search.CabinClass, search.InWeekDays,search.Equipment);
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

        private HashSet<string> normalizeSearch(IEnumerable<string> list)
        {
            if (list.Contains("All"))
                return new HashSet<string>();
            return list.ToHashSet();
        }

        bool routeFilter(IEnumerable<string> FromList, IEnumerable<string> ToList, Crawl c)
        {
            return
                c.Departure >= DateTime.Now
                && (match(FromList,c.Origin) || FromList.Contains(c.Return ? c.Route.ToAirport.Zone : c.Route.FromAirport.Zone))
                && (match(ToList, c.Destination) ||  ToList.Contains(c.Return ? c.Route.FromAirport.Zone : c.Route.ToAirport.Zone));               
        }

        Dictionary<string, IEnumerable<string>> metroCodeDictionary = new Dictionary<string, IEnumerable<string>>()
        {
            { "TYO", new []{ "HND","NRT"  } }
        };

        bool match(IEnumerable<string> collection, string code)
        {
            if (!collection.Any() || collection.Contains(code))
                return true;
            var metros = metroCodeDictionary.Where(kvp => kvp.Value.Contains(code)).Select(kvp=>kvp.Key).ToList();
            return collection.Any(s=> metros.Contains(s));                
        }




        private List<SearchResult> GetResults(IEnumerable<Crawl> outBoundList, IEnumerable<Crawl> inBoundList, uint minDays, uint maxDays,int limit, bool openJaw)
        {
            var scandi = new HashSet<string>(new[] { "CPH", "ARN", "OSL" });
            var res = new List<SearchResult>();
            var destinationHash = inBoundList.GroupBy(c =>  c.Origin).ToDictionary(g => g.Key, g => g.ToList());
            destinationHash["Scandinavia"] = inBoundList.Where(c => scandi.Contains(c.Origin)).ToList();

            foreach (var outBound in outBoundList.OrderBy(f => f.Departure.Value))
            {
                var key = openJaw && !scandi.Contains(outBound.Origin) ? "Scandinavia" : outBound.Destination;
                if (destinationHash.ContainsKey(key))
                    foreach (var inbound in destinationHash[key].OrderBy(f => f.Departure.Value))
                    {
                        var diff = inbound.Departure.Value.Subtract(outBound.Arrival.Value);
                        if (outBound.Origin == inbound.Destination || openJaw
                            && (scandi.Contains(outBound.Origin) && scandi.Contains(inbound.Destination)))
                            if (diff.TotalDays >= minDays && diff.TotalDays <= maxDays)
                            {
                                res.Add(new SearchResult { Out = outBound, In = inbound });
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
