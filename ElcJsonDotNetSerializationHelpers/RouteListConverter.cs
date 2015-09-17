using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wsdot.Elc.Contracts;

namespace Wsdot.Elc.Serialization
{
    public class RouteListConverter: JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IDictionary<string, RouteInfo>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var routeDict = value as IDictionary<string, RouteInfo>;
            if (routeDict != null)
            {
                writer.WriteStartObject();
                foreach (var routeInfo in routeDict.Select(k => k.Value))
                {
                    writer.WritePropertyName(routeInfo.Name);
                    writer.WriteStartObject();

                    if (serializer.NullValueHandling == NullValueHandling.Include || routeInfo.LrsTypes != LrsTypes.None)
                    {
                        writer.WritePropertyName("direction");
                        writer.WriteValue(Enum.GetName(typeof(LrsTypes), routeInfo.LrsTypes));
                    }

                    writer.WritePropertyName("routeType");
                    writer.WriteValue(Enum.GetName(typeof(RouteType), routeInfo.RouteType));

                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }
        }
    }
}
