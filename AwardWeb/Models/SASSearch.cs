using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AwardWeb.Models
{
    public class SASSearch
    {
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [DataType(DataType.Date)]
        [Display(Name = "Out earliest")]
        public DateTime? OutMin { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [DataType(DataType.Date)]
        [Display(Name = "Out latest")]
        public DateTime? OutMax { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [DataType(DataType.Date)]
        [Display(Name = "Inbound earliest")]
        public DateTime? InMin { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [DataType(DataType.Date)]
        [Display(Name = "Inbound latest")]
        public DateTime? InMax { get; set; }
        [Display(Name = "Days minimum")]
        public uint MinDays { get; set; }
        [Display(Name = "Days max")]
        public uint MaxDays { get; set; }
        [Display(Name = "Equipment")]
        public string Equipment { get; set; } = "";
        public uint Passengers { get; set; }
        public List<SelectListItem> Routes { get; set; }
        public List<SelectListItem> ReturnRoutes { get; set; }
        public bool Return { get; set; }
        public List<string> From { get; set; }
        public List<string> To { get; set; }
        public string Sort { get; set; }
        [Display(Name = "Cabin class")]
        public int CabinClass { get; set; }

        [Display(Name = "Open jaw")]
        public bool OpenJaw { get; set; } = true;
        public List<SelectListItem> Classes { get; internal set; }
        public List<SelectListItem> EquipmentList { get; internal set; }

        [Display(Name = "Weekdays out")]
        public List<int> OutWeekDays { get; set; }
        [Display(Name = "Weekdays in")]
        public List<int> InWeekDays { get; set; }
    }
}
