using System;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FlysasClient
{
    class Program
    {        
        public static async System.Threading.Tasks.Task<int> Main(string[] args = null)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Welcome to FlysasClient 0.9.1");
            Console.WriteLine("SAS flight data from api.flysas.com");
            Console.WriteLine("Additional offline data (airports,airlines and routes) from openflights.org");
            Console.WriteLine("");
            var data = new OpenFlightsData.OFData();
            try
            {
                data.LoadData();
            }
            catch
            {
                Console.WriteLine("Error loading Openfligts data");
            }
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",true);
            var config = builder.Build();
            Options options = new Options(config.AsEnumerable());                                  
            var client = new ConsoleClient(options,data);
            await client.InputLoop();
            return 0;
        }       
    }
}
