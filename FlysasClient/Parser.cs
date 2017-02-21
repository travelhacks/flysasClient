using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FlysasLib;


namespace FlysasClient
{
    public class ParserException : Exception
    {
        public ParserException(string s) : base(s) { }
    }

    public class Parser
    {
        Regex airportExp = new Regex("[a-zA-Z]+");
        public SASQuery Parse(string input)
        {
            var splitChars = new[] { ' ', ',', '-' };
            var parts = input.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            var stack = new Stack<string>(parts.Reverse());
            SASQuery request = null;
            if (parts.Length > 2)
            {
                request = new SASQuery();
                request.From = stack.Pop().ToUpper();
                if (airportExp.IsMatch(stack.Peek()))
                {
                    request.To = stack.Pop().ToUpper();
                    if (airportExp.IsMatch(stack.Peek()))
                    {
                        request.ReturnFrom = stack.Pop().ToUpper();
                        if (airportExp.IsMatch(stack.Peek()))
                        {

                            //req.ReturnTo = stack.Pop().ToUpper();
                            var tmp = stack.Pop().ToUpper();
                            if (tmp != request.From)
                                throw new ParserException("Must return to origin");
                        }
                    }
                }
                request.OutDate = parseDate(stack.Pop(), DateTime.Now.Date);
                if (stack.Any())
                {
                    request.InDate = parseDate(stack.Pop(), request.OutDate.Value);
                }
            }
            else
                throw new ParserException("Too few arguments");
            return request;
        }

        DateTime parseDate(string s, DateTime relativeTo)
        {
            if (s == "=")
                return relativeTo;
            var regex = new System.Text.RegularExpressions.Regex("\\+(\\d+)(.?)", RegexOptions.IgnoreCase);
            var res = regex.Match(s);
            if (res.Success)
            {
                string unit = "d";
                int val = int.Parse(res.Groups[1].Captures[0].Value);
                if (res.Groups.Count > 2)
                    unit = res.Groups[2].Captures[0].Value.ToLower();
                if (unit == "m")
                    return relativeTo.AddMonths(val);
                if (unit == "w")
                    val *= 7;
                return relativeTo.AddDays(val);
            }

            uint d, m;
            var parts = s.Split('/');
            DateTime dt;
            if (parts.Length == 2 && uint.TryParse(parts[0], out d) && uint.TryParse(parts[1], out m))
            {

                try
                {                    
                    dt = new DateTime(DateTime.Now.Year, (int)m, (int)d);
                }
                catch
                {
                    throw new ParserException("Invalid date format " + s);
                }
                if (dt.Date < DateTime.Now.Date)
                    dt = dt.AddYears(1);
            }
            else
            {
                if (!DateTime.TryParse(s, out dt))
                    throw new ParserException("Invalid date format " + s);
            }
            return dt;
        }
    }
}
