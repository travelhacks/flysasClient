using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
            return GetFrom(Return) + "-" + GetTo(Return);                
        }
        public string GetTo(bool Return)
        {
            return Return ? From : To;
        }
        public string GetFrom(bool Return)
        {
            return Return ? To : From;
        }
    }

    public class Crawl
    {
        public int Id { get; set; }
        public int RouteId { get; set; }        
        public bool Return { get; set; }
        public DateTime TravelDate { get; set; }
        public DateTime CrawlDate { get; set; }
        public DateTimeOffset? Arrival { get; set; }
        public DateTimeOffset? Departure { get; set; }
        public string Flight { get; set; }
        public virtual Route Route { get; set; }
        public int Go { get; set; }
        public int Plus { get; set; }
        public int Business { get; set; }
        public bool Success { get; set; }
    }

    public class AwardExport
    {
        public int Id { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime Updated { get; set; }
        public DateTimeOffset Arrival { get; set; }
        public DateTimeOffset Departure { get; set; }
        public int Business;
        public int Plus;
        public int Go;
    }

    

    [Table("V_Changes")]
    public class Changes
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public bool Return { get; set; }
        public DateTime TravelDate { get; set; }
        public DateTime CrawlDate { get; set; }
        public virtual Route Route { get; set; }        
        public int Business { get; set; }        
        public int? c2 { get; set; }
    }

    public class AwardContext : DbContext
    {
        public AwardContext(DbContextOptions options) : base(options)
        {           
        }
        public AwardContext(string connection) : base(new DbContextOptionsBuilder<AwardContext>().UseSqlServer(connection).Options)
        {
                    
        }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Crawl> Crawls { get; set; }
        public DbSet<Changes> Changes { get; set; }
    }
}
