using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;


namespace FlysasLib
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class UrlParameterAttribute : Attribute
    {
        public string ParameterName { get; private set; }
        public UrlParameterAttribute(string ParameterName)
        {
            this.ParameterName = ParameterName;
        }
    }

    public class SASQuery
    {

        public enum SearhMode { UNSET, POINTS, STAR, REVENUE };
        public string To { get; set; }
        public string From { get; set; }
        public string ReturnFrom { get; set; }
       // public string ReturnTo { get; set; }
        [UrlParameterAttribute("adt")]
        public int Adults { get; set; } = 1;

        [UrlParameterAttribute("chd")]
        public int Children { get; set; } = 0;
        [UrlParameterAttribute("inf")]
        public int Infants { get; set; } = 0;
        [UrlParameterAttribute("yth")]
        public int Youth { get; set; } = 0;

        [UrlParameterAttribute("lng")]
        public string Language { get; set; } = "GB";

        [UrlParameterAttribute("pos")]
        public string Position { get; set; } = "se";
        public string DisplayType { get; set; } //displayType:CALENDAR
        public string Channel { get; set; } = "web";
        public DateTime? OutDate { get; set; }
        public DateTime? InDate { get; set; }
        public SearhMode Mode;
        private string BookingFlow
        {
            get
            {
                return Mode.ToString().ToLower();
            }
        }

        public string FilterType
        {
            get
            {
                if (Mode == SearhMode.POINTS)
                    return "StandardAward";
                return null;
            }
        }

        public string FilterOn
        {
            get
            {
                if (Mode == SearhMode.POINTS)
                {
                    var str = "outbound";
                    if (InDate.HasValue)
                        str += ",inbound";
                    return str;
                }
                return null;
            }
        }

        static PropertyInfo[] properties = typeof(SASQuery).GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);

        public string GetUrl() => "https://api.flysas.com/offers/flights?" + String.Join("&", getParams());
        
        IEnumerable<string> getParams()
        {
            foreach (var property in properties)
            {
                var paramName = property.Name.camelCase();
                var attr = property.GetCustomAttribute(typeof(UrlParameterAttribute)) as UrlParameterAttribute;
                if (attr != null)                
                    paramName = attr.ParameterName;                

                var val = property.GetValue(this);
                if (val != null)
                {
                    string sVal;
                    if (property.PropertyType == typeof(DateTime?) && ((DateTime?)val).HasValue)
                        sVal = ((DateTime?)val).Value.ToString("yyyyMMddHHmm");
                    else
                        sVal = val.ToString();                    
                    yield return $"{paramName}={sVal}";
                }
            }
        }
    }
}
