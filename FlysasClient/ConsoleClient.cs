using System;
using System.Collections.Generic;
using System.Linq;

namespace FlysasClient
{
    public class ConsoleClient
    {
        public void InputLoop()
        {
            var client = new SASRestClient();
            string input = null;
            var parser = new Parser();
            while (input != "q")
            {
                Console.WriteLine("Syntax: Origin-Destination outDate [inDate]");
                Console.Write(">>");
                input = Console.ReadLine();
                SASQuery req = null;
                try
                {
                    req = parser.Parse(input);                    
                }catch(Exception ex)
                {
                    Console.Write("Error parsing input. ");
                }
                if (req != null)
                {
                    var res = client.Search(req);
                    Console.WriteLine("*********Outbound*******");
                    PrintFlights(res.outboundFlights, res.outboundFlightProducts);
                    if (req.InDate.HasValue)
                    {
                        Console.WriteLine("*********Inbound*******");
                        PrintFlights(res.inboundFlights, res.inboundFlightProducts);
                    }
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
        }
        void PrintFlights(IEnumerable<FlightBaseClass> flights, IEnumerable<FlightProductBaseClass> products)
        {
            if (flights == null)
            {
                Console.WriteLine("No flights found");
                return;
            }
            string format = "HH:mm";
            var codes = products.Select(p => p.productCode).Distinct().ToArray();
            var header = "Dep\tArr\t" + string.Join("\t", codes);
            Console.WriteLine(header);
            foreach (var r in flights)
            {
                var prices = products.Where(p => p.id.EndsWith("_" + r.id.ToString())).ToArray();
                Console.Write($"{r.startTimeInLocal.ToString(format)}\t{r.endTimeInLocal.ToString(format)}");
                foreach (var c in codes)
                {
                    var p = prices.FirstOrDefault(price => price.productCode == c);
                    Console.Write("\t" + (p != null ? p.price.formattedTotalPrice : ""));
                }
                Console.Write(Environment.NewLine);
            }
        }
    }
}
