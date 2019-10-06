using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AwardWeb.Models
{
    public class StarSearch
    {
        [Required(ErrorMessage = "IATA is required.")]        
        public string Origin { get; set; }
        [Required(ErrorMessage = "IATA is required.")]
        public string Destination { get; set; }        
        public int Pax { get; set; } = 1;
        [Display(Name = "Start date")]
        [Required(ErrorMessage = "Date is required.")]
        public DateTime OutDate { get; set; }
        public List<FlysasLib.FlightBaseClass> Results { get; set; }
        [Display(Name = "Search x days")]
        public int SearchDays { get; set; }
        [Display(Name = "Max legs")]
        public int MaxLegs { get; set; }
    }
}
