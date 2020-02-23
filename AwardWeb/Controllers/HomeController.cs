using AwardData;
using AwardWeb.Models;
using FlysasLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AwardWeb.Controllers
{

    public class HomeController : Controller
    {
        private readonly AwardContext _ctx;
        private readonly SASRestClient _client;

        public HomeController(AwardContext context, SASRestClient client)
        {
            _client = client;
            _ctx = context;
        }

        public IActionResult List([FromServices] Services.ICachedData cachedData)
        {
            var list = cachedData.Crawls.Where(c => c.Business > 0 && c.TravelDate > DateTime.Now && c.Route.Show);
            return View(list);
        }

        public IActionResult List_legacy()
        {
            return RedirectToActionPermanent(nameof(HomeController.List));
        }


        public IActionResult Console()
        {
            return View();
        }

        public IActionResult Console_legacy()
        {
            return RedirectToActionPermanent(nameof(HomeController.Console));
        }

        public IActionResult Index([FromServices] Services.ICachedData cachedData)
        {
            SASSearch sasSearch = new SASSearch();
            sasSearch.Routes = new List<SelectListItem>();
            sasSearch.ReturnRoutes = new List<SelectListItem>();
            sasSearch.Routes.Add(new SelectListItem { Value = "All", Text = "All" });
            sasSearch.Routes.Add(new SelectListItem { Value = "Europe", Text = "Scandinavia" });
            sasSearch.Routes.Add(new SelectListItem { Value = "ARN", Text = "Stockholm" });
            sasSearch.Routes.Add(new SelectListItem { Value = "CPH", Text = "Copenhagen" });
            sasSearch.Routes.Add(new SelectListItem { Value = "OSL", Text = "Oslo" });
            sasSearch.Routes.Add(new SelectListItem { Value = "Central Asia & Far East Asia", Text = "Asia" });
            sasSearch.Routes.Add(new SelectListItem { Value = "North & Central America", Text = "Usa" });
            sasSearch.Routes.Add(new SelectListItem { Value = "BOS", Text = "Boston" });
            sasSearch.Routes.Add(new SelectListItem { Value = "ORD", Text = "Chicago" });
            sasSearch.Routes.Add(new SelectListItem { Value = "LAX", Text = "Los Angeles" });
            sasSearch.Routes.Add(new SelectListItem { Value = "EWR", Text = "Newark" });
            sasSearch.Routes.Add(new SelectListItem { Value = "SFO", Text = "San Francisco" });
            sasSearch.Routes.Add(new SelectListItem { Value = "MIA", Text = "Miami" });
            sasSearch.Routes.Add(new SelectListItem { Value = "IAD", Text = "Washington" });
            sasSearch.Routes.Add(new SelectListItem { Value = "PEK", Text = "Beijing" });
            sasSearch.Routes.Add(new SelectListItem { Value = "HKG", Text = "Hong Kong" });
            sasSearch.Routes.Add(new SelectListItem { Value = "PVG", Text = "Shanghai" });
            sasSearch.Routes.Add(new SelectListItem { Value = "TYO", Text = "Tokyo" });

            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "All", Text = "All" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "Central Asia & Far East Asia", Text = "Asia" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "North & Central America", Text = "Usa" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "BOS", Text = "Boston" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "ORD", Text = "Chicago" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "LAX", Text = "Los Angeles" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "MIA", Text = "Miami" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "EWR", Text = "Newark" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "SFO", Text = "San Francisco" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "IAD", Text = "Washington" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "PEK", Text = "Beijing" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "HKG", Text = "Hong Kong" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "PVG", Text = "Shanghai" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "TYO", Text = "Tokyo" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "Europe", Text = "Scandinavia" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "ARN", Text = "Stockholm" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "CPH", Text = "Copenhagen" });
            sasSearch.ReturnRoutes.Add(new SelectListItem { Value = "OSL", Text = "Oslo" });
            sasSearch.Passengers = 1U;
            sasSearch.MinDays = 0U;
            sasSearch.MaxDays = 7U;
            sasSearch.Return = true;
            sasSearch.CabinClass = (int)CabinClass.Business;
            sasSearch.From = new List<string>(new string[] { "Europe" });
            sasSearch.To = new List<string>(new string[] { "All" });


            sasSearch.Classes = new List<SelectListItem>();
            sasSearch.Classes.Add(new SelectListItem { Text = "Business", Value = ((int)CabinClass.Business).ToString() });
            sasSearch.Classes.Add(new SelectListItem { Text = "Plus (PE)", Value = ((int)CabinClass.Plus).ToString() });
            sasSearch.Classes.Add(new SelectListItem { Text = "GO (Economy)", Value = ((int)CabinClass.Go).ToString() });
            sasSearch.Classes.Add(new SelectListItem { Text = "Any/mixed", Value = ((int)CabinClass.All).ToString() });
            sasSearch.OutWeekDays = new List<int>();
            sasSearch.InWeekDays = new List<int>();
            sasSearch.EquipmentList = cachedData.EquipmentList.Select(s => new SelectListItem(s, s)).ToList();
            sasSearch.EquipmentList.Insert(0, new SelectListItem("All", ""));
            return View(sasSearch);
        }

        public IActionResult ListResult([FromServices] Services.ICachedData cachedData, SASSearch search)
        {
            return ViewComponent(nameof(ListResult), search);
        }

        public IActionResult All()
        {
            return RedirectToActionPermanent(nameof(Index));
        }
        public IActionResult Changes()
        {
            var crawls = _ctx.Changes.Where(c => c.Route.Show).OrderByDescending(c => c.CrawlDate).Include(c => c.Route).ToList();
            return View(crawls);
        }
        public IActionResult FAQ()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FAQ(
            [FromServices] Services.IEmailSender emailSender,
            [FromServices] IOptionsSnapshot<Models.AppSettings> appSettings,
            string email,
            string message,
            int hideMe
            )
        {
            string str = "";
            if (hideMe == 4)
            {
                this._ctx.Messages.Add(new Message()
                {
                    Msg = message,
                    Email = email,
                    TimeStamp = DateTime.UtcNow
                });
                this._ctx.SaveChanges();
                str = "Thanks!";
                try
                {
                    await emailSender.SendEmailAsync(appSettings.Value.Email, "Private message from AwardHacks", email + " said " + message);
                }
                catch { }
            }
            return View((object)str);
        }


        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Star()
        {

            var search = new StarSearch { OutDate = DateTime.Now.Date.AddDays(1), Pax = 1, SearchDays = 7 };
            return View(search);
        }


        public async Task<IActionResult> StarResult(StarSearch search)
        {

            int threads = 4;
            if (ModelState.IsValid)
            {
                search.SearchDays = Math.Min(search.SearchDays, 14);
                search.Destination = search.Destination.MyTrim();
                search.Origin = search.Origin.MyTrim();
                if (search.Origin.MyLength() == 3 && search.Destination.MyLength() == 3)
                {
                    var dates = System.Linq.Enumerable.Range(0, search.SearchDays).Select(i => search.OutDate.AddDays(i)).ToList();
                    dates.Shuffle();
                    var res = new System.Collections.Concurrent.ConcurrentDictionary<DateTime, FlysasLib.SearchResult>();
                    await Dasync.Collections.ParallelForEachExtensions.ParallelForEachAsync<DateTime>(dates,
                        async date =>
                        {
                            if (!res.ContainsKey(date))//this looks smart, but doesn't realy save a lot of calls...
                            {
                                var q = new SASQuery
                                {
                                    OutDate = date,
                                    From = search.Origin,
                                    To = search.Destination,
                                    Adults = search.Pax,
                                    Mode = SASQuery.SearhMode.STAR
                                };
                                FlysasLib.SearchResult searchResult = await _client.SearchAsync(q);

                                if (searchResult.tabsInfo != null && searchResult.tabsInfo.outboundInfo != null)
                                    foreach (var dayWithNoSeats in searchResult.tabsInfo.outboundInfo.Where(tab => tab.points == 0))
                                        res.TryAdd(dayWithNoSeats.date, null);

                                res.TryAdd(date, searchResult);
                            }
                        },
                        threads,
                        false
                        );
                    search.Results = res.Where(r => r.Value?.outboundFlights != null).SelectMany(r => r.Value.outboundFlights).ToList();
                    if (search.MaxLegs > 0)
                        search.Results = search.Results.Where(r => r.segments.Count() <= search.MaxLegs).ToList();
                }
                else
                    search.Results = new List<FlysasLib.FlightBaseClass>();
            }
            return View(nameof(Star), search);
        }

        public IActionResult RedirectToFaq()
        {
            return RedirectToAction(nameof(HomeController.FAQ));
        }
        public IActionResult News()
        {
            return View();
        }
    }
}
