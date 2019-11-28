using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace AwardData
{
    public class ApplicationUser : IdentityUser
    {
        public virtual List<Alerts> Alerts { get; set; }
        public virtual List<SentMail> SentMail { get; set; }

        public string Site { get; set; }
    }
}
