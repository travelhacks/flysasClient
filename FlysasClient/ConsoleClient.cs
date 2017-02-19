using System;
using System.Collections.Generic;
using System.Linq;
using FlysasLib;

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
            string slash = "/";            
            string format = "HH:mm";
            var codes = products.Select(p => p.productCode).Distinct().ToArray();
            var first = flights.First();
            var headers = new List<string>();
            headers.Add(first.origin.code);
            headers.Add(first.destination.code);
            if(options.OutputEquipment)
                headers.Add("Equip");
            foreach(var c in codes)
            {
                headers.Add(c);
                if (options.OutputBookingClass)
                    headers.Add("");
            }
            Table table = new Table();
            table.Rows.Add(headers);            
            foreach (var r in flights)
            {
                var values = new List<string>();
                var prices = products.Where(p => p.id.EndsWith("_" + r.id.ToString())).ToArray();
                var dateDiff = (r.endTimeInLocal.Date - r.startTimeInGmt.Date).Days;
                values.Add(r.startTimeInLocal.ToString(format));
                values.Add(r.endTimeInLocal.ToString(format)+ (dateDiff > 0 ? "+" + dateDiff : ""));
                if (options.OutputEquipment)
                {
                    var s = string.Join(slash, simplify(r.segments.Select(seg => seg.airCraft.code)));
                    values.Add(s);
                }
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
                            //if (sClasses.Length >= tabLen)
                            //    sClasses = string.Join("/", p.fares.Select(f => f.bookingClass));
                        }
                        sPrice = p.price.formattedTotalPrice;
                    }
                    values.Add(sPrice);
                    if (options.OutputBookingClass)
                        values.Add(sClasses);                    
                }
                table.Rows.Add(values);                
            }
            if (options.Table)
                table.PrintTable();
            else
                table.Print();
        }

        public class Table
        {
            public List<List<string>> Rows { get; private set; } = new List<List<string>>();
            string tab = "\t";
            int tabLen = 8;
            Dictionary<int,int> dict = new Dictionary<int, int>();
            void calc()
            {
                if (Rows.Any())
                    for (int i = 0; i < Rows.First().Count; i++)
                        dict[i] = Rows.Select(r => r[i]).Select(s => s == null ? 0 : s.Length).Max();
            }

            public void Print()
            {
                calc();
                foreach(var r in Rows)
                {
                    for(int i = 0; i < r.Count;i++)
                    {
                        var s = r[i] ?? string.Empty;
                        var len = dict[i] + tabLen - dict[i] % tabLen;                        
                        var pad = (len - s.Length-1)/tabLen;
                        Console.Write(s);
                        for (int j = 0; j <= pad; j++)
                            Console.Write(tab);
                    }
                    Console.Write(Environment.NewLine);
                }                
            }
            public void PrintTable()
            {
                var pad = 2;
                calc();
                foreach (var r in Rows)
                {
                    for (int i = 0; i < r.Count; i++)
                    {
                        var s = r[i] ?? string.Empty;
                        var len = dict[i] + pad-1;                        
                        Console.Write(s);
                        for (int j = s.Length; j < len; j++)
                            Console.Write(" ");
                        Console.Write("|");                        
                    }
                    Console.Write(Environment.NewLine);
                    foreach (int i in dict.Values)
                        for(int j=0;j<i+pad;j++)
                            Console.Write("-");
                    Console.Write(Environment.NewLine);
                }
            }
        }

        private IEnumerable<string> simplify(IEnumerable<string> list)
        {
            if (list.Distinct().Count() == 1)
                yield return list.First();
            else
                foreach (string s in list)
                    yield return s;
        }
        private string limit(string s)
        {
            if (s != null && s.Length < 8)
                return s;
            return string.Empty;
        }
    }
}
