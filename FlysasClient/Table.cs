using System;
using System.Collections.Generic;
using System.Linq;

namespace FlysasClient
{

    public enum TextAlignment { Left, Right };
    public class Table
    {
        public List<List<string>> Rows { get; private set; } = new List<List<string>>();
        public Dictionary<int, TextAlignment> Alignment { get; private set; } = new Dictionary<int, TextAlignment>();
        Dictionary<int, int> dict = new Dictionary<int, int>();

        public Table(List<string> row)
        {
            Rows.Add(row);
        }

        public Table()
        {
        }
        void calc()
        {
            if (Rows.Any())
                for (int i = 0; i < Rows.First().Count; i++)
                    dict[i] = Rows.Select(r => r[i]).Select(s => s == null ? 0 : s.Length).Max();
        }

        public void Print(System.IO.TextWriter txtOut)
        {
            calc();
            int pad = 2;
            foreach (var r in Rows)
            {
                for (int i = 0; i < r.Count; i++)
                {
                    var align = Alignment.ContainsKey(i) ? Alignment[i] : TextAlignment.Left;
                    var s = r[i] ?? string.Empty;
                    var len = dict[i] + pad;
                    var padded = align == TextAlignment.Right ? s.PadLeft(len - 2) + "".PadLeft(pad) : s.PadRight(len);
                    txtOut.Write(padded);
                }
                txtOut.Write(Environment.NewLine);
            }
        }
    }
}
