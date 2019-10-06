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

            switch (orderField)
            {
                case AlertOrderField.Id:
                    list = list.OrderBy(a => a.Id).ToList();
                    break;
                case AlertOrderField.To:
                    list = list.OrderBy(a => a.Destination).ToList();
                    break;
                case AlertOrderField.From:
                    list = list.OrderBy(a => a.Origin).ToList();
                    break;
                case AlertOrderField.FromDate:
                    list = list.OrderBy(a => a.FromDate).ToList();
                    break;
                case AlertOrderField.ToDate:
                    list = list.OrderBy(a => a.ToDate).ToList();
                    break;
                case AlertOrderField.BookinClass:
                    list = list.OrderBy(a => a.CabinClass).ToList();
                    break;
                case AlertOrderField.Pax:
                    list = list.OrderBy(a => a.Passengers).ToList();
                    break;
            }
            model.Alerts = list;
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
