using FlysasLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlysasClient
{
    class Program
    {
        public static async Task<int> Main(string[] args = null)
        {
            var builder = new HostBuilder().ConfigureServices((hostcontext, services) =>
            {
                services.AddHttpClient<SASRestClient>().ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler()
                    {
                        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                    };
                });
            }).UseConsoleLifetime();

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.WriteLine("Welcome to FlysasClient 0.9.95");
                Console.WriteLine("SAS flight data from api.flysas.com");
                Console.WriteLine("Additional offline data (airports,airlines and routes) from openflights.org");
                Console.WriteLine("");
                var data = new OpenFlightsData.OFData();
                try
                {
                    data.LoadData();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading Openfligts data:" + ex.Message);
                }
                var configBuilder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true);

                var config = configBuilder.Build();
                Options options = new Options(config.AsEnumerable());
                var client = new ConsoleClient(options, data, services.GetRequiredService<SASRestClient>());
                if (args.Any())
                    await client.Run(string.Join(" ", args));
                else
                    await client.InputLoop();
                return 0;
            }
            return 0;
        }
    }
}
