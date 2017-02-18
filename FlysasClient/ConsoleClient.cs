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
            var options = new FlysasClient.Options();


            while (input != "q")
            {
                Console.WriteLine("Syntax: Origin-Destination outDate [inDate]");
                Console.Write(">>");
                input = Console.ReadLine();
                if (!options.Parse(input))
                    foreach (string query in input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        SASQuery req = null;
                        try
                        {
                            req = parser.Parse(query);
                        }
                        catch (ParserException ex)
                        {
                            Console.Write("Syntax error:" + ex.Message);
                        }
                        catch (Exception ex)
                        {
                            Console.Write("Syntax error:");
                        }
                        if (req != null)
                        {
                            var res = client.Search(req);
                            Console.WriteLine("*********Outbound*******");
                            PrintFlights(res.outboundFlights, res.outboundFlightProducts, options);
                            if (req.InDate.HasValue)
                            {
                                Console.WriteLine("*********Inbound*******");
                                PrintFlights(res.inboundFlights, res.inboundFlightProducts, options);
                            }
                        }
                        Console.Write(Environment.NewLine + Environment.NewLine);
                    }
            }

        }
        void PrintFlights(IEnumerable<FlightBaseClass> flights, IEnumerable<FlightProductBaseClass> products, FlysasClient.Options options)
        {
            bool printClasses = options.OutputBookingClass;
            string tab = "\t";
            int tabLen = 8;
            if (flights == null)
            {
                Console.WriteLine("No flights found");
                return;
            }
            string format = "HH:mm";
            var codes = products.Select(p => p.productCode).Distinct().ToArray();
            var first = flights.First();
            var header = $"{first.origin.code}{tab}{first.destination.code}{tab}" + string.Join(tab + (printClasses ? tab : ""), codes);
            Console.WriteLine(header);
            foreach (var r in flights)
            {
                var prices = products.Where(p => p.id.EndsWith("_" + r.id.ToString())).ToArray();
                var dateDiff = (r.endTimeInLocal.Date - r.startTimeInGmt.Date).Days;
                Console.Write($"{r.startTimeInLocal.ToString(format)}{tab}{r.endTimeInLocal.ToString(format)}" + (dateDiff > 0 ? "+" + dateDiff : ""));
                foreach (var c in codes)
                {
                    string sClasses = "";
                    var p = prices.FirstOrDefault(price => price.productCode == c);
                    var sPrice = "";
                    if (p != null)
                    {

                        var classes = p.fares.Select(f => f.bookingClass + f.avlSeats);
                        if (classes.Distinct().Count() == 1)
                            sClasses = classes.First();
                        else
                        {
                            sClasses = string.Join("/", classes);
                            if (sClasses.Length >= tabLen)
                                sClasses = string.Join("/", p.fares.Select(f => f.bookingClass));
                        }
                        sPrice = p.price.formattedTotalPrice;
                    }
                    Console.Write(tab + sPrice);
                    if (printClasses)
                        Console.Write(tab + sClasses);
                }
                Console.Write(Environment.NewLine);
            }
        }
    }
}
