using AwardData;
using AwardWeb.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AwardWeb.Models
{
    public class AlertMailModel
    {        
        public ApplicationUser User { get; set; }            
        public Dictionary<int, int> CountHash { get; internal set; }
        public List<MailContainer> Rows { get; set; }
    }
}
