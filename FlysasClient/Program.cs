using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FlysasClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new ConsoleClient();
            c.InputLoop();

            //bench();
            //Console.ReadKey();


        }

        private static void bench()
        {
            SASRestClient client = new SASRestClient();            
            var count = 40;
            int threads = 6;
            var watch = System.Diagnostics.Stopwatch.StartNew();            
            Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = threads },  x=>
            {
                SASQuery q = new SASQuery { From = "KLR", To = "ARN", OutDate = DateTime.Now.AddDays(1 + x).Date };
                var w2 = System.Diagnostics.Stopwatch.StartNew();
                var res = client.Search(q);                
                Console.WriteLine("Got " + res.outboundFlights?.Count + " in " + w2.Elapsed.TotalSeconds);
                
            });                              
            Console.WriteLine(watch.Elapsed.TotalSeconds);
        }
    }
}