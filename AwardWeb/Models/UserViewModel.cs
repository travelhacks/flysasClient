using AwardData;

namespace AwardWeb.Models
{
    public class UserViewModel
    {
        public ApplicationUser User { get; set; }
        public int SentCount { get; set; }
        public int AlertCount { get; set; }
    }
}