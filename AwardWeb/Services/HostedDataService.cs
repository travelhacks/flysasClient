using AwardData;
using AwardWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AwardWeb.Services
{
    public interface IDataLoader
    {
        void Reload();
    }

    public class HostedDataService : IHostedService, IDisposable
    {
        private Timer _timer;
        
        public IServiceProvider Services { get; }

        public HostedDataService(IServiceProvider services)
        {
            Services = services;
            try
            {
                DoWork(null);
            }
            catch { }
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            var ts = TimeSpan.FromSeconds(30);
            _timer = new Timer(DoWork, null, ts,ts);
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {            
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =scope.ServiceProvider.GetRequiredService<ICachedData>();
                var ctx = scope.ServiceProvider.GetRequiredService<AwardData.AwardContext>();
                try
                {
                    var data = ctx.Crawls.FromSqlRaw("select * from v_latestcrawls").
                    Include(c => c.Route).ThenInclude(r => r.FromAirport).Include(c => c.Route).ThenInclude(r => r.ToAirport).
                    Where(c => c.Flight != null && c.ExternalId == null && c.Route.Crawl && c.Route.Show).AsNoTracking().ToList();
                    scopedProcessingService.Set(data);
                } catch(Exception ex)
                {
                    //Todo: log
                }
            }
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
