using System;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FlysasClient
{
    class Program
    {        
        public static void Main(string[] args = null)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",true);
            var config = builder.Build();
            Options o = new Options(config.AsEnumerable());                                  
            var c = new ConsoleClient(o);
            c.InputLoop();
        }       
    }
}
