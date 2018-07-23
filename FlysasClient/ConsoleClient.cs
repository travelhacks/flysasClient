using System;
using System.Collections.Generic;
using System.Linq;
using FlysasLib;
using System.IO;

namespace FlysasClient
{
    public class ConsoleClient
    {
        SASRestClient client = new SASRestClient();
        Options options;
        System.IO.TextWriter txtOut = Console.Out;
        System.IO.TextReader txtIn = Console.In;
        OpenFlightsData.OFData data;


        public ConsoleClient(Options options, OpenFlightsData.OFData data)
        {
            this.options = options;
            this.data = data;
        }

        public async System.Threading.Tasks.Task InputLoop()
        {
            string input = null;
            var parser = new Parser();
            while (!nameof(Commands.Quit).Equals(input,StringComparison.CurrentCultureIgnoreCase))
            {
                txtOut.WriteLine("Syntax: Origin-Destination outDate [inDate]");
                txtOut.Write(">>");
                input = txtIn.ReadLine();
                if (!Command(input))
                    foreach (string query in input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        SASQuery req = null;
                        try
                        {
                            req = parser.Parse(query);
                            req.Mode = options.Mode;
                        }
                        catch (ParserException ex)
                        {
                            txtOut.Write("Syntax error:" + ex.Message);
                        }
                        catch
                        {
                            txtOut.Write("Syntax error:");
                        }
                        if (req != null)
                        {
                            SearchResult result = null;
                            try
                            {
                                result = await client.SearchAsync(req);
                            }
                            catch
                            {
                                txtOut.WriteLine("Error");
                            }
                            if (result != null)
                            {
                                if (result.errors != null && result.errors.Any())
                                    txtOut.WriteLine("flysas.com says: " + result.errors.First().errorMessage);
                                else
                                {
                                    txtOut.WriteLine("*********Outbound*******");
                                    PrintFlights(result.outboundFlights, options);
                                    if (req.InDate.HasValue)
                                    {
                                        txtOut.WriteLine("*********Inbound*******");
                                        PrintFlights(result.inboundFlights, options);
                                    }
                                }
                            }
                            txtOut.Write(Environment.NewLine + Environment.NewLine);
                        }
                    }
            }
        }

        enum Commands
        {
            Login, History, Logout, Points, Set, Help, Benchmark, Options, Export, Info, Quit
        };

        HashSet<Commands> requiresLogin = new HashSet<Commands>() { Commands.History, Commands.Points, Commands.Export };

        bool Command(string input)
        {
            var names = Enum.GetNames(typeof(Commands));
            var stack = new CommandStack(input);
            if (stack.Any())
            {
                var sCmd = stack.Pop();
                var name = names.FirstOrDefault(s => s.Equals(sCmd, StringComparison.CurrentCultureIgnoreCase));
                if (name != null)
                {
                    Commands cmd = (Commands)Enum.Parse(typeof(Commands), name);
                    if (!client.LoggedIn && requiresLogin.Contains(cmd))
                    {
                        txtOut.WriteLine("This feature requires login");
                        return true;
                    }
                    switch (cmd)
                    {
                        case Commands.Set:
                            options.Set(stack);
                            break;
                        case Commands.Info:
                            info(stack);
                            break;
                        case Commands.Login:
                            login(stack);
                            break;
                        case Commands.Export:
                            history(stack, true);
                            break;
                        case Commands.History:
                            history(stack, false);
                            break;
                        case Commands.Points:
                            points();
                            break;
                        case Commands.Benchmark:
                            benchMark();
                            break;
                        case Commands.Options:
                            txtOut.Write(options.Help() + Environment.NewLine);
                            break;
                        case Commands.Help:
                            txtOut.WriteLine("Commands:");
                            foreach (var s in names)
                                txtOut.WriteLine("\t" + s);
                            break;
                        case Commands.Logout:
                            client.Logout();
                            break;
                        case Commands.Quit:
                            client.Logout();
                            Environment.Exit(0);
                            break;
                    }
                    return true;
                }
            }
            return false;
        }

        private void benchMark()
        {
            var count = 40;
            int threads = 6;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            System.Threading.Tasks.Parallel.For(0, count, new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = threads }, x =>
            {
                SASQuery q = new SASQuery { From = "KLR", To = "ARN", OutDate = DateTime.Now.AddDays(1 + x).Date };
                var w2 = System.Diagnostics.Stopwatch.StartNew();
                var res = client.Search(q);
                //txtOut.WriteLine("Got " + res.outboundFlights?.Count + " in " + w2.Elapsed.TotalSeconds);

            });
            txtOut.WriteLine(watch.Elapsed.TotalSeconds);
        }

        private void points()
        {
            try
            {
                var res = client.History(1);
                txtOut.WriteLine("Status: " + res.eurobonus.currentTierName);
                txtOut.WriteLine(res.eurobonus.totalPointsForUse + " points for use");
                txtOut.WriteLine(res.eurobonus.pointsAvailable + " basic points earned this period");
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                txtOut.WriteLine("Error getting info");
                txtOut.WriteLine(ex.Message);
            }
        }

        private void info(CommandStack stack)
        {
            if (stack.Any())
            {

                var arglist = stack.ToList();
                var s = string.Join(" ", arglist);
                var airport = data.Airports.FirstOrDefault(ap => ap.IATA == s.ToUpper());
                if (airport != null)
                {
                    txtOut.WriteLine("Airport " + airport.IATA + "/" + airport.ICAO);
                    txtOut.WriteLine("Name " + airport.Name);
                    txtOut.WriteLine("City " + airport.City);
                    txtOut.WriteLine("Country " + airport.Country);
                    txtOut.WriteLine("Type " + airport.Type);
                    txtOut.WriteLine("Timezone " + airport.Timezone);
                    txtOut.WriteLine("DST " + airport.DST);
                }
                var cities = data.Airports.Where(ap => s.Equals(ap.City, StringComparison.CurrentCultureIgnoreCase)).ToList();
                if (cities.Any())
                {
                    txtOut.WriteLine("Airports in " + s);
                    foreach (var c in cities)
                        txtOut.WriteLine("\t" + c.IATA + ": " + c.Name);
                }
                var airline = data.Airlines.FirstOrDefault(al => s.Equals(al.Name, StringComparison.CurrentCultureIgnoreCase) || s.ToUpper() == al.IATA || s.ToUpper() == al.ICAO);
                if (airline != null)
                {
                    txtOut.WriteLine("Airline info for " + s);
                    txtOut.WriteLine("\t" + airline.IATA + "/" + airline.ICAO);
                    txtOut.WriteLine("\tName:" + airline.Name);
                    txtOut.WriteLine("\tCallsign:" + airline.Callsign);
                    txtOut.WriteLine("\tCountry:" + airline.Country);
                }
                var plane = data.Planes.FirstOrDefault(p =>  s.ToUpper() == p.IATA || s.ToUpper() == p.ICAO);
                if (plane != null)
                {
                    txtOut.WriteLine("Airplane info for " + s);
                    txtOut.WriteLine("\t" + plane.IATA + "/" + plane.ICAO);
                    txtOut.WriteLine("\tName:" + plane.Name);                  
                }
                if (arglist.Count >= 2)
                {

                    var orig = arglist[0];
                    var dest = arglist[1] == "-" && arglist.Count > 2 ? arglist[2] : arglist[1];
                    var routeList = data.Routes.Where(r => r.FromIATA == orig.ToUpper() && r.ToIATA == dest.ToUpper()).ToList();
                    if (routeList.Any())
                    {
                        txtOut.WriteLine("Routes from " + orig + " to " + dest);
                        foreach (var r in routeList)
                            txtOut.WriteLine("\t" + r.AirlineCode + (r.CodeShare ? " codeshare " : ""));
                    }
                }
            }
        }

        private void history(CommandStack stack, bool export)
        {
            int n = 1;
            int pages = 1;
            bool fetchAll = true;
            List<Transaction> all = new List<Transaction>();
            TransactionRoot res = null;

            if (stack.Any())
            {
                if (int.TryParse(stack.Pop(), out n) && !stack.Any())
                    fetchAll = false;
                else
                {
                    txtOut.WriteLine("Parser error");
                    return;
                }
            }
            txtOut.WriteLine("");
            do
            {
                txtOut.Write("\rFetching page " + n + (pages > 1 ? " of " + pages.ToString() : ""));
                try
                {
                    res = client.History(n);
                }
                catch (Exception ex)
                {
                    txtOut.WriteLine("Error getting page " + n);
                    txtOut.WriteLine(ex.Message);
                }
                if (res.errors != null)
                {
                    txtOut.WriteLine("Error getting page " + n + " " + res.errors.First().errorMessage);
                }
                n++;
                if (fetchAll)
                    pages = res.eurobonus.transactionHistory.totalNumberOfPages;
                if (res.errors == null && res.eurobonus != null && res.eurobonus.transactionHistory.transaction != null)
                    all.AddRange(res.eurobonus.transactionHistory.transaction);
            } while (n <= pages);
            txtOut.Write("\r");
            if (export)
            {
                var exporter = new FlightExporter();
                var list = exporter.Convert(all);
                txtOut.WriteLine("Found " + list.Count + " flight");
                if (list.Any())
                    try
                    {
                        exporter.SaveCSV(list);
                        txtOut.WriteLine("Files saved");
                    }
                    catch (Exception ex)
                    {

                    }
            }
            else
            {
                Table t = new Table();
                foreach (var r in all)
                {
                    var values = new List<string>();
                    values.Add(r.datePerformed.ToString("yyyy-MM-dd"));
                    values.Add(r.typeOfTransaction);
                    values.Add(r.basicPointsAfterTransaction);
                    values.Add(r.availablePointsAfterTransaction.ToString());
                    values.Add(r.description);

                    t.Rows.Add(values);

                }

                t.Alignment[3] = TextAlignment.Right;
                t.Print(txtOut);
                if (fetchAll)
                {
                    txtOut.WriteLine("Summary");
                    t = new Table();
                    foreach (var g in all.GroupBy(trans => trans.typeOfTransaction))
                        t.Rows.Add(new List<string>(new[] { g.Key, g.Sum(trans => trans.availablePointsAfterTransaction).ToString() }));
                    t.Alignment[1] = TextAlignment.Right;
                    t.Print(txtOut);
                }
            }
        }

        private void login(CommandStack stack)
        {
            var u = options.UserName;
            var p = options.Password;
            if (u.IsNullOrWhiteSpace() && stack.Any())
                u = stack.Pop();
            else
            {
                txtOut.WriteLine("User: ");
                u = txtIn.ReadLine();
            }

            if (p.IsNullOrWhiteSpace() && stack.Any())
                p = stack.Pop();
            else
            {
                txtOut.WriteLine("Enter password: ");
                p = getPassword();
            }
            try
            {
                var result = client.Login(u, p);
                txtOut.WriteLine("Login for " + u + " " + (result ? " success" : "failed"));
            }
            catch (Exception ex)
            {
                txtOut.WriteLine("Login failed");
            }
        }

        private string getPassword()
        {
            string str = "";
            ConsoleKeyInfo key;
            while (true)
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (str.Any())
                    {
                        str = str.Substring(0, str.Length - 1);
                        txtOut.Write("\b \b");
                    }
                }
                else
                {
                    if (key.Key == ConsoleKey.Enter)
                        break;
                    else
                    {
                        str += key.KeyChar;
                        txtOut.Write("*");
                    }
                }
            }
            txtOut.WriteLine();
            return str;
        }

        void PrintFlights(IEnumerable<FlightBaseClass> flights, FlysasClient.Options options)
        {
            string separator = "/";
            string timeFormat = "HH:mm";
            var products = flights.Where(f=>f.cabins!=null).SelectMany(f=>f.cabins.AllProducts);
            var codes = products.Select(p=>p.productCode).Distinct().ToArray();
            var first = flights.First();
            var headers = new List<string>();
            headers.Add(first.origin.code);
            headers.Add(first.destination.code);
            if (options.OutputEquipment)
                headers.Add("Equip");
            if (options.OutputFlightNumber)
                headers.Add("Flight");
            Table table = new Table();
            foreach (var c in codes)
            {
                headers.Add(c);
                table.Alignment[headers.Count - 1] = TextAlignment.Right;
                if (options.OutputBookingClass)
                    headers.Add("");
            }
            table.Rows.Add(headers);
            foreach (var r in flights)
            {
                var values = new List<string>();
                var dateDiff = (r.endTimeInLocal.Date - r.startTimeInGmt.Date).Days;
                values.Add(r.startTimeInLocal.ToString(timeFormat));
                values.Add(r.endTimeInLocal.ToString(timeFormat) + (dateDiff > 0 ? "+" + dateDiff : ""));
                if (options.OutputEquipment)
                    values.Add(r.segments.Select(seg => seg.airCraft.code).SimplifyAndJoin(separator));
                if (options.OutputFlightNumber)
                    values.Add(r.segments.Select(seg => seg.marketingCarrier.code + seg.flightNumber).SimplifyAndJoin(separator));

                foreach (var c in codes)
                {
                    string sClasses = "";
                    var p = r.cabins?.AllProducts.FirstOrDefault(prod => prod.productCode == c);
                    var sPrice = "";
                    var pax = "";
                    if (p != null)
                    {
                        var classes = p.fares.Select(f => f.bookingClass + f.avlSeats);
                        pax = p.fares.Min(f=>f.avlSeats).ToString();
                        sClasses = classes.SimplifyAndJoin(separator);
                        sPrice = p.price.formattedTotalPrice;
                    }
                    if (options.Mode != "REVENUE")
                        values.Add(pax);
                    else
                        values.Add(sPrice);
                    if (options.OutputBookingClass)
                        values.Add(sClasses);
                }
                table.Rows.Add(values);
            }
            table.Print(txtOut);
        }

    }
}