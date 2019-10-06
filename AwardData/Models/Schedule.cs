using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AwardData
{
    public class Schedule
    {
        [Key, Column(Order = 0)]
        public int RouteId { get; set; }
        [Key, Column(Order = 1)]
        public bool Return { get; set; }
        [Key, Column(Order = 2)]
        public int Day { get; set; }
    }
}
