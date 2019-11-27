using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;


/* Response classes generated with http://json2csharp.com/ and then manually edited */

namespace FlysasLib
{
    public class Price
    {
        public string currency { get; set; }
        public double basePrice { get; set; }
        public double totalTax { get; set; }
        public double totalPrice { get; set; }
        public string formattedBasePrice { get; set; }
        public string formattedTotalTax { get; set; }
        public string formattedTotalPrice { get; set; }
        public int points { get; set; }
        public int credits { get; set; }
    }


    public class PricePerPassengerType
    {
        public int id { get; set; }
        public string type { get; set; }
        public int numberCount { get; set; }
        public Price price { get; set; }
    }

    public class PriceList : Price
    {
        public List<PricePerPassengerType> pricePerPassengerType { get; set; }
    }

    public class Fare
    {
        public string segmentId { get; set; }
        public string fareClass { get; set; }
        public string bookingClass { get; set; }
        public int avlSeats { get; set; }
    }
    
    public class Error
    {
        public string errorCode;
        public string errorMessage;
    }

    public class FlightProductBaseClass
    {
        public string id { get; set; }
        public string productName { get; set; }
        public string productCode { get; set; }
        public DateTime date { get; set; }
        public bool lowestFare { get; set; }
        public int recoId { get; set; }
        public int recoFlightId { get; set; }
        public PriceList price { get; set; }
        public List<Fare> fares { get; set; }
        public string fareKey { get; set; }
        public bool isStandardAward { get; set; }
        public string productType { get; set; }
        public string productSubtype { get; set; }
        public bool sasRecommended { get; set; }     
    }

    public class Via : KeyValuePair
    {
        public string haltDuration { get; set; }
    }

    public class Segment
    {
        public int id { get; set; }
        public string arrivalTerminal { get; set; }
        public DateTimeOffset? arrivalDateTimeInLocal { get; set; }
        public DateTimeOffset? arrivalDateTimeInGmt { get; set; }
        public DateTimeOffset? departureDateTimeInLocal { get; set; }
        public DateTimeOffset? departureDateTimeInGmt { get; set; }
        public KeyValuePair departureAirport { get; set; }
        public KeyValuePair arrivalAirport { get; set; }
        public KeyValuePair departureCity { get; set; }
        public KeyValuePair arrivalCity { get; set; }
        public KeyValuePair airCraft { get; set; }
        public string flightNumber { get; set; }
        public string duration { get; set; }
        public KeyValuePair marketingCarrier { get; set; }
        public KeyValuePair operatingCarrier { get; set; }
        public double onTimePerformance { get; set; }
        public int miles { get; set; }
        public int numberOfStops { get; set; }
        public string departureTerminal { get; set; }
    }

    public class FlightBaseClass
    {
        public int id { get; set; }
        public KeyValuePair origin { get; set; }
        public KeyValuePair destination { get; set; }
        public KeyValuePair originCity { get; set; }
        public KeyValuePair destinationCity { get; set; }
        public string connectionDuration { get; set; }
        public DateTimeOffset startTimeInLocal { get; set; }
        public DateTime startTimeInGmt { get; set; }
        public DateTimeOffset endTimeInLocal { get; set; }
        public DateTime endTimeInGmt { get; set; }
        public int stops { get; set; }
        public List<Via> via { get; set; }
        public List<Segment> segments { get; set; }
        public LowestFares lowestFares { get; set; }        
        public Cabins cabins { get; set; }
    }

    public class Cabins
    {
        [JsonProperty(propertyName: "BUSINESS")]
        [JsonConverter(typeof(ProductArrayConverter))]
        public List<FlightProductBaseClass> business { get; set; }
        [JsonProperty(propertyName: "ECONOMY")]
        [JsonConverter(typeof(ProductArrayConverter))]
        public List<FlightProductBaseClass> economy { get; set; }
        [JsonProperty(propertyName: "GO")]
        [JsonConverter(typeof(ProductArrayConverter))]
        public List<FlightProductBaseClass> go { get; set; }
        [JsonProperty(propertyName: "PLUS")]
        [JsonConverter(typeof(ProductArrayConverter))]
        public List<FlightProductBaseClass> plus { get; set; }

        public IEnumerable<FlightProductBaseClass> AllProducts
        {
            get
            {
                var list = new List<FlightProductBaseClass>();
                foreach (var prop in this.GetType().GetProperties().Where(pi => pi.PropertyType == typeof(List<FlightProductBaseClass>)))
                {
                    var val = prop.GetValue(this);
                    if (val != null)
                        list.AddRange(val as List<FlightProductBaseClass>);
                }
                return list;
            }
        }

    }
    

   

    public class KeyValuePair
    {
        public string code { get; set; }
        public string name { get; set; }
    }    

    public class Currency : KeyValuePair
    {        
        public string symbol { get; set; }
        public string decimalSeperator { get; set; }
        public string position { get; set; }
        public string currencyDelimiter { get; set; }
    }

    public class Link
    {
        public string rel { get; set; }
        public string href { get; set; }
    }



    


    public class SearchResult : RootBaseClass
    {
        //Not used
        //productInfo
        //links
        //outboundLowestFare

        [JsonConverter(typeof(FlightBaseClassConverter))]
        public List<FlightBaseClass> outboundFlights { get; set; }
        [JsonConverter(typeof(FlightBaseClassConverter))]
        public List<FlightBaseClass> inboundFlights { get; set; }
        
        
        public Currency currency { get; set; }
        public List<Link> links { get; set; }
        public string regionName { get; set; }
        public string offerId { get; set; }       
        public bool isOutboundIntercontinental { get; set; }
        public string pricingType { get; set; }

        public TabsInfo tabsInfo { get; set; }
        public List<Error> errors { get; set; }
    }

    public class BestPrice 
    {
        public string product { get; set; }
        public string productId { get; set; }
        public int avlSeats { get; set; }        
        //Todo: prices / points
    }

    public class LowestFares
    {
        [JsonProperty(propertyName: "BUSINESS")]
        public BestPrice business { get; set; }
        [JsonProperty(propertyName: "ECONOMY")]
        public BestPrice economy { get; set; }
    }

    public class AuthResponse : RootBaseClass
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string jti { get; set; }        
        //for logins
        public string refresh_token;
        public string customerSessionId;
        public string sessionId;
        public string error;
    }

    public class RootBaseClass
    {        
        public string json;
        public bool httpSuccess;
    }

    public class Transaction
    {
        static System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"([A-Z]{2}) ?(\d*)\w?\w? ?(.+)");
        System.Text.RegularExpressions.Match m;
        System.Text.RegularExpressions.Match match
        {
            get
            {
                //Booked awardfligts has the same syntax as flown legs but with no flight number and negative points
                if (m == null && description2 != null && typeOfTransaction.Equals("Flight Activity", StringComparison.OrdinalIgnoreCase) && availablePointsAfterTransaction > 0)
                    m = regex.Match(description2);
                return m;
            }
        }
        public string id { get; set; }
        public DateTime datePerformed { get; set; }
        public string description1 { get; set; }
        public string description2 { get; set; }
        public int availablePointsAfterTransaction { get; set; }
        public string basicPointsAfterTransaction { get; set; }
        public string typeOfTransaction { get; set; }

        public string Origin
        {
            get{ return description1.Split('-').First();}                 
        }
        public string Destination
        {
            get { return description1.Split('-').Last(); }
        }
        public string Airline
        {
            get { return getMatch(1); }
        }
        public string FlightNumber
        {
            get { return getMatch(2); }
        }

        public string CabinClass
        {
            get { return getMatch(3); }
        }
        string getMatch(int group)
        {            
                if (match != null && match.Success && match.Groups.Count>group)
                    return match.Groups[group].Captures[0].Value;
                return "";          
        }
    }

    public class TransactionHistory
    {
        public int totalNumberOfPages { get; set; }
        public List<Transaction> transaction { get; set; }
    }

    public class EuroBonus
    {
      public string euroBonusId { get; set; }
      public string nameOnCard { get; set; }
      public DateTime dateRegistered { get; set; }
      public DateTime qualifyingPeriodStartDate { get; set; }
      public DateTime qualifyingPeriodEndDate { get; set; }
      public int pointsAvailable { get; set; }
      public int totalPointsForUse { get; set; }
      public string currentTierCode { get; set; }
      public string currentTierName { get; set; }
        
      public TransactionHistory transactionHistory { get; set; }      
    }

    public class AwardProduct
    {
        public int points { get; set; }
        public string id { get; set; }
        public double totalTax { get; set; }
    }

    public class Products
    {
        [JsonProperty(propertyName: "SAS GO")]
        public AwardProduct GO { get; set; }
        [JsonProperty(propertyName: "SAS PLUS")]
        public AwardProduct PLUS { get; set; }
        [JsonProperty(propertyName: "SAS BUSINESS")]
        public AwardProduct BUSINESS { get; set; }
    }


    public class AwardSegment
    {
        public string org { get; set; }
        public string dest { get; set; }
        public DateTimeOffset startTimeLocal { get; set; }
        public DateTimeOffset endTimeInLocal { get; set; }
        public string flightNumber { get; set; }
    }

    public class AwardFlight
    {
        public int flightId { get; set; }
        public string org { get; set; }
        public string dest { get; set; }
        public Products products { get; set; }
        public List<AwardSegment> segments { get; set; }
        public string totalDuration { get; set; }
    }

    public class AwardResult : RootBaseClass
    {
        public string offerId { get; set; }
        public string currency { get; set; }
        public string pos { get; set; }
        public List<AwardFlight> outbounds { get; set; }
        public string redirectUrl { get; set; }
    }

    public class TransactionRoot : RootBaseClass
    {
        public EuroBonus eurobonus { get; set; }
        public List<Error> errors { get; set; }
        public List<Error> errorInfo
        {
            get
            {
                return errors;
            }
            set
            {
                errors = value;
            }
        }
    }

    public class TabsInfo
    {
        public List<TabInfo> outboundInfo { get; set; }
        public List<TabInfo> inboundInfo { get; set; }
    }

    public class TabInfo
    {
        public DateTime date { get; set; }
        public string price { get; set; }
        public double points { get; set; }
    }

}
