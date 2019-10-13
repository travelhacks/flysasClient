using System;
using AwardData;
namespace AwardWeb.Code
{
    public class Utils
    {
        public static string CreateUrl(string site,  Crawl outbound, Crawl inbound ,BookingClass bclass, uint pax)
        {
            if(string.IsNullOrWhiteSpace(site))
            {
                site = "https://sas.se";
            }
            var lang = site.Contains("flysas.com", StringComparison.InvariantCultureIgnoreCase) ? "gb-en" : "en";
            var shortClass = ClassStringShort(bclass);
            var longClass = ClassStringLong(bclass);
            bool roundtrip = inbound != null;
            var url = site+ $"/{lang}/book/flights?search=" + (roundtrip ? "RT_" : "OW_");            
            url += $"{outbound.Origin}-{outbound.Destination}-{outbound.TravelDate.ToString("yyyyMMdd")}";
            if (roundtrip)
                url += $"-{inbound.TravelDate.ToString("yyyyMMdd")}";
            url += $"_a{pax}c0i0y0&view=upsell&bookingFlow=points&out_flight_number={outbound.Flight}&out_sub_class={longClass}&out_class={shortClass}";
            if (roundtrip)
                url += $"&in_flight_number={inbound.Flight}&in_sub_class={longClass}&in_class={shortClass}";
            bool hasLink = !roundtrip || outbound.Origin == inbound.Destination;            
            return hasLink ? url : null;
        }

        public static string CreateUrl(string site, AllChanges outbound, AllChanges inbound, BookingClass bclass, uint pax)
        {
            return CreateUrl(site, clone(outbound), clone(inbound), bclass, pax);
        }

        private static Crawl clone(AllChanges org)
        {
            if (org == null)
                return null;
            return new Crawl { Id = org.Id, RouteId = org.RouteId, Route = org.Route, CrawlDate = org.CrawlDate, TravelDate = org.TravelDate };
        }

        public static string ClassStringShort(BookingClass cabin)
        {
            if (cabin == BookingClass.All)
                return "";
            return cabin.ToString().ToUpper();
        }
        public static string ClassStringLong(BookingClass cabin)
        {
            var tmp = ClassStringShort(cabin);
            if (!string.IsNullOrEmpty(tmp))
                return "SAS " + tmp;
            return tmp;
        }
    }
}
