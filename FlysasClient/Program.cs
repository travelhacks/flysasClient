using System;
using System.Threading.Tasks;
using FlysasLib;
using Microsoft.Extensions.Configuration;
using System.IO;




namespace FlysasClient
{
    class Program
    {
        


        public static void Main(string[] args = null)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var config = builder.Build();
            Options o = new Options(config.AsEnumerable());                                  
            var c = new ConsoleClient(o);
            c.InputLoop();



        }       
    }
}