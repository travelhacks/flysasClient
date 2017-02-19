using System;
using System.Collections.Generic;


/* Response classes generated with http://json2csharp.com/ and then manually edited */

namespace FlysasClient
{

    public class FlightProduct
    {
        public string flightProductId { get; set; }
        public int recoId { get; set; }
        public int recoFlightId { get; set; }
    }

    public class AssociatedProducts
    {
        public List<FlightProduct> flightProducts { get; set; }
    }

    public class Offer
    {
        public string id { get; set; }
        public int recoFlightId { get; set; }
        public string boundType { get; set; }
        public bool lowestFare { get; set; }
        public string productName { get; set; }
        public string fareKey { get; set; }
        public AssociatedProducts associatedProducts { get; set; }
    }

    public class Price2
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
        public Price2 price { get; set; }
    }

    public class Price : Price2
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

    public class Connection
    {
        public int id { get; set; }
        public string boundType { get; set; }
        public int flightId { get; set; }
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
        public Price price { get; set; }
        public List<Fare> fares { get; set; }
        public string fareKey { get; set; }
        public List<Connection> connections { get; set; }
        public bool flyingToOrOverRussia { get; set; }
    }




  


    public class Via : KVP
    {
        public string haltDuration { get; set; }
    }

    
    public class Segment
    {
        public int id { get; set; }
        public string arrivalTerminal { get; set; }
        public string arrivalDateTimeInLocal { get; set; }
        public string arrivalDateTimeInGmt { get; set; }
        public string departureDateTimeInLocal { get; set; }
        public string departureDateTimeInGmt { get; set; }
        public KVP departureAirport { get; set; }
        public KVP arrivalAirport { get; set; }
        public KVP departureCity { get; set; }
        public KVP arrivalCity { get; set; }
        public KVP airCraft { get; set; }
        public string flightNumber { get; set; }
        public string duration { get; set; }
        public KVP marketingCarrier { get; set; }
        public KVP operatingCarrier { get; set; }
        public double onTimePerformance { get; set; }
        public int miles { get; set; }
        public int numberOfStops { get; set; }
        public string departureTerminal { get; set; }
    }
    
    public class FlightBaseClass
    {
        public int id { get; set; }
        public KVP origin { get; set; }
        public KVP destination { get; set; }
        public DateTime connectionDuration { get; set; }
        public DateTimeOffset startTimeInLocal { get; set; }
        public DateTime startTimeInGmt { get; set; }
        public DateTimeOffset endTimeInLocal { get; set; }
        public DateTime endTimeInGmt { get; set; }
        public int stops { get; set; }
        public List<Via> via { get; set; }
        public List<Segment> segments { get; set; }
    }

    
    

    public class KVP
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class Weight
    {
        public int checkedBag { get; set; }
        public int cabinBag { get; set; }
        public string unit { get; set; }
    }

    public class Baggage
    {
        public int checkedBag { get; set; }
        public int cabinBag { get; set; }
        public string unit { get; set; }
        public List<Weight> weight { get; set; }
    }

    public class Amenities
    {
        public string meal { get; set; }
        public bool lounge { get; set; }
        public bool fastTrack { get; set; }
        public string comfort { get; set; }
        public bool refund { get; set; }
        public bool rebook { get; set; }
        public Baggage baggage { get; set; }
        public string refundRebookMsg { get; set; }
    }

    public class ProductInfo
    {
        public string productName { get; set; }
        public string description { get; set; }
        public string displayType { get; set; }
        public int rank { get; set; }
        public int order { get; set; }
        public Amenities amenities { get; set; }
    }

    public class TabsInfo
    {
        public string boundType { get; set; }
        public int noOfTabsBefore { get; set; }
        public int noOfTabsAfter { get; set; }
    }

    public class Currency
    {
        public string code { get; set; }
        public string name { get; set; }
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

    public class SearchResult
    {
        public string pricingType { get; set; }
        public List<Offer> offers { get; set; }
        public List<FlightProductBaseClass> outboundFlightProducts { get; set; }
        public List<FlightProductBaseClass> inboundFlightProducts { get; set; }
        public List<FlightBaseClass> outboundFlights { get; set; }
        public List<FlightBaseClass> inboundFlights { get; set; }
        public List<ProductInfo> productInfo { get; set; }
        public List<TabsInfo> tabsInfo { get; set; }
        public Currency currency { get; set; }
        public List<Link> links { get; set; }
        public string regionName { get; set; }
        public string offerId { get; set; }
    }
    public class AuthResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string jti { get; set; }
    }

}
