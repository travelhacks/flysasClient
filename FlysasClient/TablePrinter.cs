using FlysasLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlysasClient
{
    public class TablePrinter
    {
        private readonly System.IO.TextWriter textWriter;

        public TablePrinter(System.IO.TextWriter textWriter)
        {
            this.textWriter = textWriter;
        }

        public void PrintFlights(IEnumerable<FlightBaseClass> flights, FlysasClient.Options options)
        {
            string separator = "/";
            string timeFormat = "HH:mm";
            var products = flights.Where(f => f.cabins != null).SelectMany(f => f.cabins.AllProducts);
            var sorter = new ProductComparer();
            var codes = products.OrderBy(s => s, sorter).Select(p => p.productCode).Distinct().ToArray();
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
                if(options.Mode.Equals("POINTS", StringComparison.OrdinalIgnoreCase))
                    headers.Add("Tax");
                if (options.OutputBookingClass)
                    headers.Add("");
            }
            table.Rows.Add(headers);
            foreach (var r in flights.OrderBy(f => f.startTimeInGmt).ThenBy(f => f.endTimeInGmt))
            {
                var values = new List<string>();
                var dateDiff = (r.endTimeInLocal.Date - r.startTimeInLocal.Date).Days;
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
                        pax = p.fares.Min(f => f.avlSeats).ToString();
                        sClasses = classes.SimplifyAndJoin(separator);
                        sPrice = p.price.formattedTotalPrice;
                    }
                    if (!options.Mode.Equals("revenue", StringComparison.OrdinalIgnoreCase))
                        values.Add(pax);
                    if (!options.Mode.Equals("star", StringComparison.OrdinalIgnoreCase))
                        values.Add(sPrice);
                    if (options.OutputBookingClass)
                        values.Add(sClasses);
                }
                table.Rows.Add(values);
            }
            table.Print(this.textWriter);
        }
    }
}
