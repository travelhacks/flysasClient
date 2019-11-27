using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace FlysasClient
{
    class Program
    {        
        public static async System.Threading.Tasks.Task<int> Main(string[] args = null)
        {
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
            catch(Exception ex)
            {
                Console.WriteLine("Error loading Openfligts data:" + ex.Message);
            }
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true);
            var config = builder.Build();
            Options options = new Options(config.AsEnumerable());                                  
            var client = new ConsoleClient(options,data);
            if (args.Any())
                await client.Run(string.Join(" ", args ));
            else
                await client.InputLoop();
            return 0;
        }       
    }
}
