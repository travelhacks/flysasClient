using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AwardData;
using AwardWeb.Models;


namespace AwardWeb.Controllers
{

    public class AlertController : Controller
    {
        AwardData.AwardContext ctx;
        AppSettings appSettings;

        public AlertController(AwardData.AwardContext context, Microsoft.Extensions.Options.IOptionsMonitor<AppSettings> settingsAccessor)
        {
            ctx = context;
            appSettings = settingsAccessor.CurrentValue;
        }
      
        public async Task<IActionResult> SendAlerts(
            [FromServices]Services.IViewRenderService render,
            [FromServices]Services.IEmailSender sender,            
            bool dryRun = true, string secret = null)
        {
            if (secret == appSettings.Secret)
            {
                var changes = ctx.AllChanges.Include(c => c.Route).Where(c => c.Route.Crawl && c.Route.Show && (c.Business > 0 || c.Go > 0 || c.Plus >0))
                    .ToList().Where(c=>c.HasIncrease(BookingClass.All) ).ToList();
                var alerts = ctx.Alerts.Where(a => !a.ToDate.HasValue || a.ToDate >= DateTime.Now.Date).ToList();
                var newMails = new Dictionary<string, AlertMailModel>();
                foreach (var change in changes)
                {
                    var matches = alerts.Where(a => a.Created < change.CrawlDate && a.RouteId == change.RouteId && a.Return == change.Return && a.IsInRange(change.TravelDate) );
                    matches = matches.Where(a => a.Matches(BookingClass.Go, change.Go) || a.Matches(BookingClass.Plus, change.Plus) || a.Matches(BookingClass.Business, change.Business)).ToList();
                    foreach (var alert in matches)
                    {
                        if (change.HasIncrease(alert.CabinClass))
                            if (dryRun || !ctx.SentMail.Any(sm => sm.UserId == alert.UserId && sm.Crawl.RouteId == alert.RouteId && sm.Crawl.Return == alert.Return && sm.Crawl.TravelDate == change.TravelDate))
                            {
                                if (!newMails.ContainsKey(alert.UserId))
                                {
                                    var user = ctx.Users.FirstOrDefault(u => u.Id == alert.UserId);
                                    newMails[alert.UserId] = new AlertMailModel
                                    {
                                        User = user,
                                        Rows = new List<MailContainer>()
                                    };
                                }
                                newMails[alert.UserId].Rows.Add(new MailContainer { Crawl = change, Pax = (uint)alert.Passengers, CabinClass = alert.CabinClass });
                            }
                    }
                }
                var countHash = newMails.Values.SelectMany(m => m.Rows).GroupBy(c => c.Crawl.Id).ToDictionary(g => g.Key, g => g.Count());
                if (dryRun)
                {
                    foreach (var mail in newMails.Values.Take(5))
                    {
                        mail.CountHash = countHash;
                        var html = await render.RenderToStringAsync("Alerts/AlertMail", mail);
                        await sender.SendEmailAsync(appSettings.Email, "AwardHacks found new seats for you", html);
                    }
                }
                else
                    foreach (var mail in newMails.Values)
                    {

                        try
                        {
                            mail.CountHash = countHash;
                            mail.Rows = mail.Rows.OrderBy(c => c.Crawl.TravelDate).Distinct().ToList();
                            var html = await render.RenderToStringAsync("Home/AlertMail", mail);

                            foreach (var c in mail.Rows.Select(r => r.Crawl))
                            {
                                var sm = new SentMail
                                {
                                    UserId = mail.User.Id,
                                    CrawlId = c.Id,
                                    Date = DateTime.UtcNow

                                };
                                ctx.SentMail.Add(sm);
                            }
                            ctx.SaveChanges();

                            await sender.SendEmailAsync(mail.User.Email, "AwardHacks found new seats for you", html);

                        }
                        catch (Exception ex)
                        {
                            //Todo: log
                        }
                        //return Content(html, "text/html");
                    }
            }
            return Ok();
        }


        public IActionResult TestMail()
        {
            var mail = new AlertMailModel();
            mail.User = new ApplicationUser { UserName = "Test" };
            mail.Rows = ctx.AllChanges.Include(c => c.Route).Where(c => c.Route.Crawl && c.Route.Show).ToList().Where(c=>c.HasIncrease(BookingClass.All)).
                ToList().OrderByDescending(c => c.CrawlDate).Take(2).Select(c => new MailContainer { Crawl = c, CabinClass = BookingClass.Business, Pax = 1 }).ToList();
            mail.CountHash = mail.Rows.GroupBy(c => c.Crawl.Id).ToDictionary(g => g.Key, g => g.Count());
            return View("/Views/Alerts/AlertMail.cshtml", mail);
        }
    }
}
    
