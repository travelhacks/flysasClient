using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;


namespace FlysasLib
{
    public abstract class QueryBase
    {
        protected IEnumerable<string> getParams()
        {
            foreach (var property in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var val = property.GetValue(this);
                if (val != null)
                {
                    string sVal;
                    if (property.PropertyType == typeof(DateTime?) && ((DateTime?)val).HasValue)
                        sVal = ((DateTime?)val).Value.ToString("yyyyMMdd");
                    else
                        sVal = val.ToString();
                    var paramName = property.Name.First().ToString().ToLower() + property.Name.Substring(1);
                    yield return $"{paramName}={sVal}";
                }
            }
        }
        public abstract string GetUrl();
    }

    public class SASQuery : QueryBase
    {
        public string To { get; set; }
        public string From { get; set; }
        public string ReturnFrom { get; set; }
        // public string ReturnTo { get; set; }
        public int Adt { get; set; } = 1;
        public string BookingFlow { get; set; } = "REVENUE";
        public string Lng { get; set; } = "GB";
        public string Pos { get; set; } = "se";
        public string DisplayType { get; set; } //displayType:CALENDAR
        public string Channel { get; set; }// = "web";
        public DateTime? OutDate { get; set; }
        public DateTime? InDate { get; set; }

        public override string GetUrl()
        {
            return "https://api.flysas.com/offers/flightproducts?" + String.Join("&", getParams());
        }
    }
    public class AwardQuery : QueryBase
    {
        public DateTime? OutDate { get; set; }
        public DateTime? InDate { get; set; }
        public string Org { get; set; }
        public string Dest { get; set; }
        public int Adt { get; set; } = 1;
        public int Chd { get; set; } = 0;
        public int Inf { get; set; } = 0;

        public override string GetUrl()
        {
            return "https://labs.flysas.com/labsapi/v1.0/awardCache/flightproducts?pos=SE&" + String.Join("&", getParams());
        }
    }
}