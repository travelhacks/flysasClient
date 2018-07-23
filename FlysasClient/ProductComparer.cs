using FlysasLib;
using System;
using System.Collections.Generic;


namespace FlysasClient
{
    public class ProductComparer : IComparer<FlightProductBaseClass>
    {
        static List<string> order = new List<string>(
          new[]   { "ECICNBG", "ECOD","ECOB","ECOA","ECOQ", "EBSARGO","ECONOMY",
                        "PREMD", "PREMM", "PREML","EBSARP"
                        ,"BUSB", "BUSA", "EBSARB","BUSINESS" }
        );
        public int Compare(FlightProductBaseClass p1, FlightProductBaseClass p2)
        {
            return getVal(p1).CompareTo(getVal(p2));
        }
        private int getVal(FlightProductBaseClass product)
        {
            return order.FindIndex(code => code == product.productCode);                        
        }
    }
}
