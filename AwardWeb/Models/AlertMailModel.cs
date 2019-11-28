using AwardData;
using System.Collections.Generic;

namespace AwardWeb.Models
{
    public class AlertMailModel
    {
        public ApplicationUser User { get; set; }
        public Dictionary<int, int> CountHash { get; internal set; }
        public List<MailContainer> Rows { get; set; }
    }
}
