using AwardData;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AwardWeb.Models
{
    public class AlertModel
    {
        //public List<Alerts> Alerts { get; set; }
        public Alerts NewAlert { get; set; }
        public List<SelectListItem> Classes { get; internal set; }
        public string Json { get; set; }
        public AlertOrderField OrderField { get; internal set; }
        public bool Descending { get; internal set; }
    }
}
