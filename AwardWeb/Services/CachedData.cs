using AwardData;
using System.Collections.Generic;
using System.Linq;

namespace AwardWeb.Services
{
    public class CachedData : ICachedData
    {

        public List<AwardData.Crawl> Crawls { get; private set; } = new List<Crawl>();

        public List<string> EquipmentList { get; private set; } = new List<string>();

        public void Set(List<Crawl> crawls)
        {
            Crawls = crawls.OrderBy(c => c.Departure).ToList();
            EquipmentList = Crawls.Select(c => c.Equipment).Distinct().OrderBy(s => s).ToList();
        }
    }
}
