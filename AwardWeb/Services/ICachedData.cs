using System.Collections.Generic;

namespace AwardWeb.Services
{
    public interface ICachedData
    {
        List<AwardData.Crawl> Crawls { get; }
        List<string> EquipmentList { get; }
        void Set(List<AwardData.Crawl> crawls);
    }
}
