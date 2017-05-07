using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace AwardData
{
    public class Route
    {
        public int Id { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public virtual List<Crawl> Crawls { get; set; }

        public string ToString(bool Return)
        {
            if (Return)
                return $"{To}-{From}";
            return $"{From}-{To}";
        }
    }

    public class Crawl
    {
        public int Id { get; set; }
        public int RouteId { get; set; }        
        public bool Return { get; set; }
        public DateTime TravelDate { get; set; }
        public DateTime CrawlDate { get; set; }
        public virtual Route Route { get; set; }
        public int Go { get; set; }
        public int Plus { get; set; }
        public int Business { get; set; }
        public bool Success { get; set; }
    }

    public class AwardContext : DbContext
    {
        public AwardContext(DbContextOptions options) : base(options)
        {           
        }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Crawl> Crawls { get; set; }
    }
}
