using AwardData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AwardWeb.Services
{
    public interface ICachedData
    {        
        List<AwardData.Crawl> Crawls { get; }
        List<string> EquipmentList { get; }
        void Set(List<AwardData.Crawl> crawls);
    }
}
