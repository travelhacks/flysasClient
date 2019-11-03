using System;
using AwardData;
using FlysasLib;
namespace AwardWeb.Code
{
    public class Utils
    {
        public static string CreateUrl(string site,  Crawl outbound, Crawl inbound ,CabinClass bclass, uint pax)
        {
            if(site.IsNullOrWhiteSpace())            
                site = "https://sas.se";            
            var lang = site.Contains("flysas.com", StringComparison.InvariantCultureIgnoreCase) ? "gb-en" : "en";
            var shortClass = ClassStringShort(bclass);
            var longClass = ClassStringLong(bclass);
            bool roundtrip = inbound != null;
            bool destinationOpenJaw = roundtrip && outbound.Destination != inbound.Origin && outbound.Origin == inbound.Destination;
            var url = new System.Text.StringBuilder(site + $"/{lang}/book/flights?search=");
            url.Append(roundtrip ? destinationOpenJaw ? "OJ" : "RT" : "OW");            
            url.Append( $"_{outbound.Origin}-{outbound.Destination}-{outbound.TravelDate.ToString("yyyyMMdd")}");
            if (roundtrip)
            {
                if (destinationOpenJaw)
                    url.Append($"_{inbound.Origin}-{inbound.Destination}");
                url.Append($"-{inbound.TravelDate.ToString("yyyyMMdd")}");
            }
            url.Append($"_a{pax}c0i0y0&view=upsell&bookingFlow=points&out_flight_number={outbound.Flight}&out_sub_class={longClass}&out_class={shortClass}");
            if (roundtrip)
                url.Append($"&in_flight_number={inbound.Flight}&in_sub_class={longClass}&in_class={shortClass}");
            bool hasLink = !roundtrip || outbound.Origin == inbound.Destination;            
            return hasLink ? url.ToString() : null;
        }

        public static string CreateUrl(string site, AllChanges outbound, AllChanges inbound, CabinClass bclass, uint pax)
        {
            return CreateUrl(site, clone(outbound), clone(inbound), bclass, pax);
        }

        private static Crawl clone(AllChanges org)
        {
            if (org == null)
                return null;
            return new Crawl { Id = org.Id, RouteId = org.RouteId, Route = org.Route, CrawlDate = org.CrawlDate, TravelDate = org.TravelDate };
        }

        public static string ClassStringShort(CabinClass cabin)
        {
            if (cabin == CabinClass.All)
                return "";
            return cabin.ToString().ToUpper();
        }
        public static string ClassStringLong(CabinClass cabin)
        {
            var tmp = ClassStringShort(cabin);
            if (!string.IsNullOrEmpty(tmp))
                return "SAS " + tmp;
            return tmp;
        }
    }
}
