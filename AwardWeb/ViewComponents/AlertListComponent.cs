using AwardData;
using AwardWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AwardWeb
{    
    public class AlertListViewComponent : ViewComponent
    {
        AwardContext ctx;
        public AlertListViewComponent(AwardContext ctx)
        {
            this.ctx = ctx;
        }

        public Task<IViewComponentResult> InvokeAsync(string userEmail, AlertOrderField orderField= AlertOrderField.Id, bool descending = false)
        {
            var model = new AlertListModel();
            var query = ctx.Alerts.Include(a => a.Route).Where(a => a.User.Email == userEmail).OrderByDescending(a => a.Id);
            var list = query.ToList();

            model.Alerts = orderField switch
            {
                AlertOrderField.Id => list.OrderBy(a => a.Id).ToList(),
                AlertOrderField.To => list.OrderBy(a => a.Destination).ToList(),
                AlertOrderField.From => list.OrderBy(a => a.Origin).ToList(),
                AlertOrderField.FromDate => list.OrderBy(a => a.FromDate).ToList(),
                AlertOrderField.ToDate => list.OrderBy(a => a.ToDate).ToList(),
                AlertOrderField.BookinClass => list.OrderBy(a => a.CabinClass).ToList(),
                AlertOrderField.Pax => list.OrderBy(a => a.Passengers).ToList()
            };             
            model.Descending = descending;
            model.OrderField = orderField;
            if (descending || orderField == AlertOrderField.Id)
                model.Alerts = model.Alerts.Reverse().ToList();

            return Task.FromResult<IViewComponentResult>(View(model));
        }

    }
    public enum AlertOrderField
    {
        Id,
        To,
        From,
        FromDate,
        ToDate,
        BookinClass,
        Pax
    }
    public class AlertListModel
    {
     
        public AlertOrderField OrderField { get; set; }
        public bool Descending { get; set; }
        public IEnumerable<Alerts> Alerts { get; set; }
    }
}
