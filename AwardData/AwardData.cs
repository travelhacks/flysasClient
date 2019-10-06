using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AwardData
{

    public class AwardContext : IdentityDbContext<ApplicationUser>
    {
        public AwardContext(DbContextOptions options) : base(options)
        {
        }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Schedule>().HasKey(table => new { table.RouteId, table.Return, table.Day });
            modelBuilder.Entity<ZoneCosts>().HasKey(table => new { table.origin, table.destination, table.booking_class, table.bonus_program });
            modelBuilder.Entity<Airport>().HasKey(t => t.IATA);
            modelBuilder.Entity<Route>().HasOne(t => t.FromAirport).WithMany(t2=>t2.RoutesFrom).HasForeignKey(t=>t.From);
            modelBuilder.Entity<Route>().HasOne(t => t.ToAirport).WithMany(t2 => t2.RoutesTo).HasForeignKey(t => t.To);
        }

        public DbSet<Route> Routes { get; set; }
        public DbSet<Crawl> Crawls { get; set; }
        public DbSet<Changes> Changes { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ZoneCosts> ZoneCosts { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Alerts> Alerts { get; set; }
        public DbSet<SentMail> SentMail { get; set; }
        public DbSet<AllChanges> AllChanges { get; set; }

    }
}
