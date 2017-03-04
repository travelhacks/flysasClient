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
        Options options = new FlysasClient.Options();
        System.IO.TextWriter txtOut = Console.Out;
        System.IO.TextReader txtIn = Console.In;
        public ConsoleClient(Options options)
        {
            this.options = options;
        }

        public void InputLoop()
        {
            string input = null;
            var parser = new Parser();
            while (input != "q")
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
                        }
                        catch (ParserException ex)
                        {
                            txtOut.Write("Syntax error:" + ex.Message);
                        }
                        catch (Exception ex)
                        {
                            txtOut.Write("Syntax error:");
                        }
                        if (req != null)
                        {
                            var res = client.Search(req);
                            if (res == null)                             
                                txtOut.WriteLine("Error");
                            else
                                if (res.errors != null && res.errors.Any())                              
                                    txtOut.WriteLine("flysas.com says: " + res.errors.First().errorMessage);                            
                            else
                            {
                                txtOut.WriteLine("*********Outbound*******");
                                PrintFlights(res.outboundFlights, res.outboundFlightProducts, options);
                                if (req.InDate.HasValue)
                                {
                                    txtOut.WriteLine("*********Inbound*******");
                                    PrintFlights(res.inboundFlights, res.inboundFlightProducts, options);
                                }
                            }
                        }
                        txtOut.Write(Environment.NewLine + Environment.NewLine);
                    }
            }
        }

        enum Commands
        {
            Login, History, Logout, Points, Set, Help, Benchmark, Options
        };

        HashSet<Commands> requiresLogin = new HashSet<Commands>() { Commands.History, Commands.Points };

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


                        case Commands.Login:
                            {
                                bool result;
                                var u = options.UserName;
                                var p = options.Password;
                                if(string.IsNullOrEmpty(u))
                                {
                                    txtOut.WriteLine("User: ");
                                    u = txtIn.ReadLine();
                                }
                                if (string.IsNullOrEmpty(p))
                                {
                                    p = "";
                                    txtOut.WriteLine("Enter password: ");
                                    ConsoleKeyInfo key;                                    
                                    while (true)
                                    {                                        
                                        key = Console.ReadKey(true);
                                        if (key.Key == ConsoleKey.Backspace)
                                        {
                                            if (p.Length > 0)
                                            {
                                                p = p.Substring(0,p.Length - 1);
                                                txtOut.Write("\b \b");
                                            }
                                        }                                       
                                        else
                                        {
                                            if (key.Key == ConsoleKey.Enter)
                                                break;
                                            else
                                            {
                                                p += key.KeyChar;
                                                txtOut.Write("*");
                                            }
                                        }
                                    }
                                    txtOut.WriteLine();
                                }
                                try
                                {
                                    result = client.Login(u, p);
                                    txtOut.WriteLine("Login for " + u + " " + (result ? " success" : "failed"));
                                }
                                catch (Exception ex)
                                {
                                    txtOut.WriteLine("Login failed");
                                }
                            }
                            break;
                        case Commands.History:
                            {
                                int n = 1;
                                int pages = 1;
                                bool fetchAll = true;
                                if (stack.Any())
                                {
                                    int.TryParse(stack.Pop(), out n);
                                    fetchAll = false;
                                }
                                List<Transaction> all = new List<Transaction>();
                                TransactionRoot res = null;
                                Table t = new Table();
                                txtOut.WriteLine("");
                                do
                                {
                                    txtOut.Write("\rFetching page " + n + (pages > 1 ? " of " + pages.ToString() : ""));
                                    try
                                    {
                                        res = client.History(n);
                                    }
                                    catch (MyHttpException ex)
                                    {
                                        txtOut.WriteLine("Error getting page " + n);
                                        txtOut.WriteLine(ex.Message);
                                    }
                                    n++;
                                    if (fetchAll)
                                        pages = res.eurobonus.transactionHistory.totalNumberOfPages;
                                    if (res.errors == null && res.eurobonus != null && res.eurobonus.transactionHistory.transaction != null)
                                    {
                                        all.AddRange(res.eurobonus.transactionHistory.transaction);

                                        foreach (var r in res.eurobonus.transactionHistory.transaction)
                                        {
                                            var values = new List<string>();
                                            values.Add(r.datePerformed.ToString("yyyy-MM-dd"));
                                            values.Add(r.typeOfTransaction);
                                            values.Add(r.basicPointsAfterTransaction);
                                            values.Add(r.availablePointsAfterTransaction.ToString());
                                            values.Add(r.description);
                                            t.Rows.Add(values);
                                        }
                                    }
                                } while (n <= pages);
                                txtOut.Write("\r");
                                t.Print(txtOut);
                                if (fetchAll)
                                    foreach (var g in all.GroupBy(trans => trans.typeOfTransaction))
                                        txtOut.WriteLine(g.Key + "\t" + g.Sum(trans => trans.availablePointsAfterTransaction));
                            }
                            break;
                        case Commands.Points:
                            {
                                try
                                {
                                    var res = client.History(1);
                                    txtOut.WriteLine("Status: " + res.eurobonus.currentTierCode);
                                    txtOut.WriteLine(res.eurobonus.totalPointsForUse + " points for use");
                                    txtOut.WriteLine(res.eurobonus.pointsAvailable + " basic points earned this period");
                                }
                                catch (System.Net.Http.HttpRequestException ex)
                                {
                                    txtOut.WriteLine("Error getting info");
                                    txtOut.WriteLine(ex.Message);

                                }
                            }
                            break;
                        case Commands.Benchmark:
                            var count = 40;
                            int threads = 6;
                            var watch = System.Diagnostics.Stopwatch.StartNew();
                            System.Threading.Tasks.Parallel.For(0, count, new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = threads }, x =>
                            {
                                SASQuery q = new SASQuery { From = "KLR", To = "ARN", OutDate = DateTime.Now.AddDays(1 + x).Date };
                                var w2 = System.Diagnostics.Stopwatch.StartNew();
                                var res = client.Search(q);
                                Console.WriteLine("Got " + res.outboundFlights?.Count + " in " + w2.Elapsed.TotalSeconds);

                            });
                            Console.WriteLine(watch.Elapsed.TotalSeconds);
                            break;
                        case Commands.Options:
                            txtOut.Write(options.Help() + Environment.NewLine);
                            break;
                        case Commands.Help:
                            txtOut.WriteLine("Commands:");
                            foreach (var s in names)
                                txtOut.WriteLine("\t" + s);
                            break;
                    }
                    return true;
                }
            }
            return false;
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
            if (options.OutputEquipment)
                headers.Add("Equip");
            foreach (var c in codes)
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
                values.Add(r.endTimeInLocal.ToString(format) + (dateDiff > 0 ? "+" + dateDiff : ""));
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
                            sClasses = string.Join("/", classes);                            
                        sPrice = p.price.formattedTotalPrice;
                    }
                    values.Add(sPrice);
                    if (options.OutputBookingClass)
                        values.Add(sClasses);
                }
                table.Rows.Add(values);
            }
            if (options.Table)
                table.PrintTable(txtOut);
            else
                table.Print(txtOut);
        }

        public class Table
        {
            public List<List<string>> Rows { get; private set; } = new List<List<string>>();
            string tab = "\t";
            int tabLen = 8;
            Dictionary<int, int> dict = new Dictionary<int, int>();
            void calc()
            {
                if (Rows.Any())
                    for (int i = 0; i < Rows.First().Count; i++)
                        dict[i] = Rows.Select(r => r[i]).Select(s => s == null ? 0 : s.Length).Max();
            }

            public void Print(TextWriter txtOut)
            {
                calc();
                foreach (var r in Rows)
                {
                    for (int i = 0; i < r.Count; i++)
                    {
                        var s = r[i] ?? string.Empty;
                        var len = dict[i] + tabLen - dict[i] % tabLen;
                        var pad = (len - s.Length - 1) / tabLen;
                        txtOut.Write(s);
                        for (int j = 0; j <= pad; j++)
                            txtOut.Write(tab);
                    }
                    txtOut.Write(Environment.NewLine);
                }
            }
            public void PrintTable(TextWriter txtOut)
            {
                var pad = 2;
                calc();
                foreach (var r in Rows)
                {
                    for (int i = 0; i < r.Count; i++)
                    {
                        var s = r[i] ?? string.Empty;
                        var len = dict[i] + pad - 1;
                        txtOut.Write(s);
                        for (int j = s.Length; j < len; j++)
                            txtOut.Write(" ");
                        txtOut.Write("|");
                    }
                    txtOut.Write(Environment.NewLine);
                    foreach (int i in dict.Values)
                        for (int j = 0; j < i + pad; j++)
                            txtOut.Write("-");
                    txtOut.Write(Environment.NewLine);
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
    }
}
