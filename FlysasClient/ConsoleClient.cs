using FlysasLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlysasClient
{
    public class ConsoleClient
    {
        private readonly SASRestClient _client;
        private readonly Options _options;
        private readonly System.IO.TextWriter _txtOut = Console.Out;
        private readonly System.IO.TextReader _txtIn = Console.In;
        private readonly OpenFlightsData.OFData _data;

        enum Commands
        {
            Login, History, Logout, Points, Set, Help, Options, Export, Info, Quit, Calendar
        };

        readonly HashSet<Commands> requiresLogin = new HashSet<Commands>() { Commands.History, Commands.Points, Commands.Export, Commands.Calendar };

        public ConsoleClient(Options options, OpenFlightsData.OFData data, SASRestClient sasRestClient)
        {
            _options = options;
            _data = data;
            _client = sasRestClient;
        }

        public async System.Threading.Tasks.Task InputLoop()
        {
            string input = null;
            while (!nameof(Commands.Quit).Equals(input, StringComparison.OrdinalIgnoreCase))
            {
                _txtOut.WriteLine("Syntax: Origin-Destination outDate [inDate]");
                _txtOut.Write(">>");
                input = _txtIn.ReadLine();
                await Run(input);
            }
        }

        public async System.Threading.Tasks.Task Run(string input)
        {
            var parser = new Parser();
            foreach (string query in input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!Command(query))
                {
                    SASQuery req = null;
                    try
                    {
                        req = parser.Parse(query.Trim());
                        req.Mode = _options.Mode;
                    }
                    catch (ParserException ex)
                    {
                        _txtOut.Write("Syntax error:" + ex.Message);
                    }
                    catch
                    {
                        _txtOut.Write("Syntax error:");
                    }
                    if (req != null)
                    {
                        SearchResult result = null;
                        try
                        {
                            result = await _client.SearchAsync(req);
                        }
                        catch
                        {
                            _txtOut.WriteLine("Error");
                        }
                        if (result != null)
                        {
                            if (result.errors != null && result.errors.Any())
                                _txtOut.WriteLine("flysas.com says: " + result.errors.First().errorMessage);
                            else
                            {
                                var printer = new TablePrinter(_txtOut);
                                _txtOut.WriteLine("*********Outbound*******");
                                printer.PrintFlights(result.outboundFlights, _options, req.From, req.To);
                                if (req.InDate.HasValue)
                                {
                                    _txtOut.WriteLine("*********Inbound*******");
                                    printer.PrintFlights(result.inboundFlights, _options, req.To, req.From);
                                }
                            }
                        }
                        _txtOut.Write(Environment.NewLine + Environment.NewLine);
                    }
                }
            }
        }

        bool Command(string input)
        {
            var names = Enum.GetNames(typeof(Commands));
            var stack = new CommandStack(input);
            if (stack.Any())
            {
                var sCmd = stack.Pop();
                var name = names.FirstOrDefault(s => s.Equals(sCmd, StringComparison.OrdinalIgnoreCase));
                if (name != null)
                {
                    Commands cmd = (Commands)Enum.Parse(typeof(Commands), name);
                    if (!_client.LoggedIn && requiresLogin.Contains(cmd))
                    {
                        _txtOut.WriteLine("This feature requires login");
                        return true;
                    }
                    switch (cmd)
                    {
                        case Commands.Set:
                            if (!_options.Set(stack))
                                _txtOut.WriteLine("Error: Unknown settings");
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
                        case Commands.Options:
                            _txtOut.Write(_options.Help() + Environment.NewLine);
                            break;
                        case Commands.Help:
                            _txtOut.WriteLine("Commands:");
                            foreach (var s in names)
                                _txtOut.WriteLine("\t" + s);
                            break;
                        case Commands.Logout:
                            _client.Logout();
                            break;
                        case Commands.Quit:
                            _client.Logout();
                            Environment.Exit(0);
                            break;
                        case Commands.Calendar:
                            Calendar();
                            break;
                    }
                    return true;
                }
            }
            return false;
        }

        private void Calendar()
        {
            ReservationsResult.Reservations reservations = _client.MyReservations();
            if (reservations.ReservationsReservations.Any())
            {
                foreach (ReservationsResult.Reservation reservation in reservations.ReservationsReservations)
                {
                    _txtOut.Write("Booking reference: ");
                    _txtOut.WriteLine(reservation.AirlineBookingReference);
                    _txtOut.Write($"Destination: {reservation.Connections[0].Destination.AirportCode}");
                    _txtOut.WriteLine($" Was written to your export folder as {reservation.AirlineBookingReference}.ICS");
                    _txtOut.WriteLine("Just drag it into your calender app.");

                    FlysasLib.CalendarPrinter cp = new CalendarPrinter();
                    cp.WriteICal(reservation);

                }

            }
            else
                _txtOut.WriteLine("Sorry: No bookings found!");
        }

        private void points()
        {
            try
            {
                var res = _client.History(1);
                _txtOut.WriteLine("Status: " + res.eurobonus.currentTierName);
                _txtOut.WriteLine(res.eurobonus.totalPointsForUse + " points for use");
                _txtOut.WriteLine(res.eurobonus.pointsAvailable + " basic points earned this period");
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                _txtOut.WriteLine("Error getting info");
                _txtOut.WriteLine(ex.Message);
            }
        }

        private void info(CommandStack stack)
        {
            if (stack.Any())
            {

                var arglist = stack.ToList();
                var s = string.Join(" ", arglist);
                var airport = _data.Airports.FirstOrDefault(ap => ap.IATA == s.ToUpper());
                if (airport != null)
                {
                    _txtOut.WriteLine($"Airport: {airport.IATA}/{airport.ICAO}");
                    _txtOut.WriteLine($"Name: {airport.Name}");
                    _txtOut.WriteLine($"City: {airport.City}");
                    _txtOut.WriteLine($"Country: {airport.Country}");
                    _txtOut.WriteLine($"Type: {airport.Type}");
                    _txtOut.WriteLine($"Timezone {airport.Timezone}");
                    _txtOut.WriteLine($"Daylight saving time: {airport.DST}");
                }
                var cities = _data.Airports.Where(ap => s.Equals(ap.City, StringComparison.OrdinalIgnoreCase)).ToList();
                if (cities.Any())
                {
                    _txtOut.WriteLine("Airports in " + s);
                    foreach (var c in cities)
                        _txtOut.WriteLine("\t" + c.IATA + ": " + c.Name);
                }
                var airline = _data.Airlines.FirstOrDefault(al => s.Equals(al.Name, StringComparison.OrdinalIgnoreCase) || s.ToUpper() == al.IATA || s.ToUpper() == al.ICAO);
                if (airline != null)
                {
                    _txtOut.WriteLine("Airline info for " + s);
                    _txtOut.WriteLine($"\t{airline.IATA}/{airline.ICAO}");
                    _txtOut.WriteLine($"\tName: {airline.Name}");
                    _txtOut.WriteLine($"\tCallsign: {airline.Callsign}");
                    _txtOut.WriteLine($"\tCountry: {airline.Country}");
                }
                var plane = _data.Planes.FirstOrDefault(p => s.ToUpper() == p.IATA || s.ToUpper() == p.ICAO);
                if (plane != null)
                {
                    _txtOut.WriteLine("Airplane info for " + s);
                    _txtOut.WriteLine($"\t{plane.IATA}/{plane.ICAO}");
                    _txtOut.WriteLine($"\tName: {plane.Name}");
                }
                if (arglist.Count >= 2)
                {

                    var orig = arglist[0];
                    var dest = arglist[1] == "-" && arglist.Count > 2 ? arglist[2] : arglist[1];
                    var routeList = _data.Routes.Where(r => r.FromIATA == orig.ToUpper() && r.ToIATA == dest.ToUpper()).ToList();
                    if (routeList.Any())
                    {
                        _txtOut.WriteLine($"Routes from {orig} to {dest}");
                        foreach (var r in routeList)
                            _txtOut.WriteLine("\t" + r.AirlineCode + (r.CodeShare ? " codeshare " : ""));
                    }
                }
            }
        }

        private void history(CommandStack stack, bool export)
        {
            int page = 1;
            int pages = 1;
            bool fetchAll = true;
            List<Transaction> all = new List<Transaction>();
            TransactionRoot res = null;

            if (stack.Any())
            {
                if (int.TryParse(stack.Pop(), out page) && !stack.Any())
                    fetchAll = false;
                else
                {
                    _txtOut.WriteLine("Parser error");
                    return;
                }
            }
            _txtOut.WriteLine("");
            do
            {
                _txtOut.Write($"\rFetching page { page}{(pages > 1 ? " of " + pages : "")}");
                try
                {
                    res = _client.History(page);
                }
                catch (Exception ex)
                {
                    _txtOut.WriteLine($"Error getting page {page}");
                    _txtOut.WriteLine(ex.Message);
                }
                if (res.errors != null)
                {
                    _txtOut.WriteLine($"Error getting page {page} {res.errors.First().errorMessage}");
                }
                page++;
                if (fetchAll)
                    pages = res.eurobonus.transactionHistory.totalNumberOfPages;
                if (res.errors == null && res.eurobonus != null && res.eurobonus.transactionHistory.transaction != null)
                    all.AddRange(res.eurobonus.transactionHistory.transaction);
            } while (page <= pages);
            _txtOut.Write("\r");
            if (export)
            {
                var exporter = new FlightExporter();
                var list = exporter.Convert(all);
                _txtOut.WriteLine($"Found {list.Count} flights");
                if (list.Any())
                    exporter.SaveCSV(list);
                _txtOut.WriteLine("Files saved");
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
                    values.Add(r.description1 + " " + r.description2);

                    t.Rows.Add(values);

                }

                t.Alignment[3] = TextAlignment.Right;
                t.Print(_txtOut);
                if (fetchAll)
                {
                    _txtOut.WriteLine("Summary");
                    t = new Table();
                    foreach (var g in all.GroupBy(trans => trans.typeOfTransaction))
                        t.Rows.Add(new List<string>(new[] { g.Key, g.Sum(trans => trans.availablePointsAfterTransaction).ToString() }));
                    t.Alignment[1] = TextAlignment.Right;
                    t.Print(_txtOut);
                }
            }
        }

        private void login(CommandStack stack)
        {
            var userName = _options.UserName;
            var passWord = _options.Password;
            if (userName.IsNullOrWhiteSpace())
                if (stack.Any())
                    userName = stack.Pop();
                else
                {
                    _txtOut.WriteLine("User: ");
                    userName = _txtIn.ReadLine();
                }

            if (passWord.IsNullOrWhiteSpace())
                if (stack.Any())
                    passWord = stack.Pop();
                else
                {
                    _txtOut.WriteLine("Enter password: ");
                    passWord = getPassword();
                }
            try
            {
                var result = _client.Login(userName, passWord);
                _txtOut.WriteLine($"Login for {userName}  {(result ? " success" : "failed")}");
            }
            catch (Exception)
            {
                _txtOut.WriteLine("Login failed");
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
                        _txtOut.Write("\b \b");
                    }
                }
                else
                {
                    if (key.Key == ConsoleKey.Enter)
                        break;
                    else
                    {
                        str += key.KeyChar;
                        _txtOut.Write("*");
                    }
                }
            }
            _txtOut.WriteLine();
            return str;
        }
    }
}