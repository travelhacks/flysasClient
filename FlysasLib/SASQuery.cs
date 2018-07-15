using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;


namespace FlysasLib
{
    public class SASQuery
    {
        public string To { get; set; }
        public string From { get; set; }
        public string ReturnFrom { get; set; }
       // public string ReturnTo { get; set; }
        public int Adt { get; set; } = 1;       
        public string Lng { get; set; } = "GB";
        public string Pos { get; set; } = "se";
        public string DisplayType { get; set; } //displayType:CALENDAR
        public string Channel { get; set; }// = "web";
        public DateTime? OutDate { get; set; }
        public DateTime? InDate { get; set; }
        public bool Award;
        private string BookingFlow
        {
            get
            {
                return Award ? "STAR" : "REVENUE"; //POINTS
            } 
        }
        
            


        static PropertyInfo[] properties = typeof(SASQuery).GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);


        public string GetUrl()
        {
            //return "https://api.flysas.com/offers/flightproducts?" + String.Join("&", getParams());
            return "https://api.flysas.com/offers/flights?" + String.Join("&", getParams());
        }

        IEnumerable<string> getParams()
        {
            foreach (var property in properties)
            {
                var val = property.GetValue(this);
                if (val != null)
                {
                    string sVal;
                    if (property.PropertyType == typeof(DateTime?) && ((DateTime?)val).HasValue)
                        sVal = ((DateTime?)val).Value.ToString("yyyyMMddHHmm");
                    else
                        sVal = val.ToString();
                    var paramName = property.Name.First().ToString().ToLower() + property.Name.Substring(1);
                    yield return $"{paramName}={sVal}";
                }
            }
        }
    }
}
