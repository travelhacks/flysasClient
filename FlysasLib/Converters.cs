using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FlysasLib
{
    public class FlightBaseClassConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(FlightBaseClass).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var start = reader.Path;
            var regex = new System.Text.RegularExpressions.Regex("^" + reader.Path + "\\.F\\d+$");
            var list = new List<FlightBaseClass>();
            if (reader.Read())
                while (!(reader.Path == start && reader.TokenType == JsonToken.EndObject) && reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject && regex.IsMatch(reader.Path))
                    {
                        var flight = serializer.Deserialize<FlightBaseClass>(reader);
                        list.Add(flight);
                    }
                }
            return list;
        }        

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class ProductArrayConverter : JsonConverter
    {


        public override bool CanConvert(Type objectType)
        {
            return typeof(FlightProductBaseClass).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var start = reader.Path;
            int startLevel = reader.Depth;
            var list = new List<FlightProductBaseClass>();
            var regex = new System.Text.RegularExpressions.Regex("\\.products\\.[IO]_\\d+$");
            if (reader.Read())
                while (!(reader.Path == start && reader.TokenType == JsonToken.EndObject) && reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                             if (reader.Depth - startLevel == 3 && regex.IsMatch(reader.Path) )
                        {
                            var product = serializer.Deserialize<FlightProductBaseClass>(reader);
                            list.Add(product);
                        }

                }
            return list;
        }


        public override bool CanWrite => false;
        

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

}
