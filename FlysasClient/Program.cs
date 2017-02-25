using System;
using System.Linq;
using System.Threading.Tasks;
using FlysasLib;

namespace FlysasClient
{
    class Program
    {
        static void Main(string[] args)
        {

            //bench();
            //////benchJson();
            //Console.ReadKey();
            ////return;

            var c = new ConsoleClient();
            c.InputLoop();



        }

        private static void benchJson()
        {
            SASRestClient client = new SASRestClient();
            SASQuery q = new SASQuery { From = "KLR", To = "ARN", OutDate = DateTime.Now.AddDays(1).Date };            
            var res = client.Search(q);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for(int i = 0; i < 10000; i++)
            {
                var foo = Newtonsoft.Json.JsonConvert.DeserializeObject<SearchResult>(res.json);
            }
            Console.WriteLine(watch.Elapsed.TotalSeconds);
        }

        private static void bench()
        {
            SASRestClient client = new SASRestClient();
            client.AnonymousLogin();
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