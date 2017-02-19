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
                            if (res.errors != null && res.errors.Any())
                                Console.WriteLine("flysas.com says: " + res.errors.First().errorMessage);
                            else
                            {
                                Console.WriteLine("*********Outbound*******");
                                PrintFlights(res.outboundFlights, res.outboundFlightProducts, options);
                                if (req.InDate.HasValue)
                                {
                                    Console.WriteLine("*********Inbound*******");
                                    PrintFlights(res.inboundFlights, res.inboundFlightProducts, options);
                                }
                            }
                        }
                        Console.Write(Environment.NewLine + Environment.NewLine);
                    }
            }

        }
        void PrintFlights(IEnumerable<FlightBaseClass> flights, IEnumerable<FlightProductBaseClass> products, FlysasClient.Options options)
        {         
            string tab = "\t";
            int tabLen = 8;         
            string format = "HH:mm";
            var codes = products.Select(p => p.productCode).Distinct().ToArray();
            var first = flights.First();
            var headers = new List<string>();
            headers.Add(first.origin.code);
            headers.Add(first.destination.code);
            if(options.OutputEquipment)
                headers.Add("Equip");
            headers.AddRange(codes.Select(c => c + (options.OutputBookingClass ? tab : string.Empty)));
            var header = string.Join(tab,headers);
            Console.WriteLine(header);
            foreach (var r in flights)
            {
                var values = new List<string>();
                var prices = products.Where(p => p.id.EndsWith("_" + r.id.ToString())).ToArray();
                var dateDiff = (r.endTimeInLocal.Date - r.startTimeInGmt.Date).Days;
                values.Add(r.startTimeInLocal.ToString(format));
                values.Add(r.endTimeInLocal.ToString(format)+ (dateDiff > 0 ? "+" + dateDiff : ""));                
                if (options.OutputEquipment)
                    values.Add(r.segments.First().airCraft.code);
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
                    values.Add(sPrice);
                    if (options.OutputBookingClass)
                        values.Add(sClasses);                    
                }                
                Console.WriteLine(string.Join(tab,values));
            }
        }
    }
}
