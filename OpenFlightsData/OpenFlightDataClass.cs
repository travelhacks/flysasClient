using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenFlightsData
{
    public class OpenFlightDataClass
    {
        protected double myDouble(string s)
        {
            double.TryParse(s, out double d);
            return d;
        }

        protected int? myInt(string s)
        {
            if (int.TryParse(s, out int i))
                return new int?(i);
            return new int?();
        }

        protected string[] mySplit(string str)
        {

            var list = new List<string>();
            bool open = false;
            var builder = new StringBuilder();
            foreach (char c in str.ToCharArray().Concat(new[] { ',' }))
                if (c == ',' && !open)
                {
                    list.Add(builder.ToString());
                    builder.Clear();
                }
                else
                {
                    if (c == '"')
                        open = !open;
                    else
                        builder.Append(c);
                }
            return list.ToArray();
        }
      
        protected string GetPath(string fileName)
        {
            return System.IO.Path.Combine(System.IO.Path.Combine(System.AppContext.BaseDirectory, "data"), fileName);
        }
    }
}
