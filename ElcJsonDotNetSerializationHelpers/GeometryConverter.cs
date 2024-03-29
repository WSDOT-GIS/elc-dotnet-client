﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using Wsdot.Geometry.Contracts;

namespace Wsdot.Elc.Serialization
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Allows a <see cref="GeometryContract"/> (an abstract class) to be deserialized by <see href="http://json.net/">Json.NET</see>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>This code comes from Tom DuPoint .NET <see href="http://www.tomdupont.net/2014/04/deserialize-abstract-classes-with.html"/>.</remarks>
    public abstract class AbstractJsonConverter<T> : JsonConverter
    {

        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            var typeInfo = typeof(T).GetTypeInfo();
            return typeInfo.IsAssignableFrom(objectType.GetTypeInfo());
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            var jObject = JObject.Load(reader);

            T target = Create(objectType, jObject);
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }



        protected static bool FieldExists(JObject jObject, string name, JTokenType type)
        {
            if (jObject == null) {
                throw new ArgumentNullException(nameof(jObject));
            }
            return jObject.TryGetValue(name, out JToken token) && token.Type == type;
        }

    }

    public class GeometryConverter : AbstractJsonConverter<GeometryContract>
    {
        protected override GeometryContract Create(Type objectType, JObject jObject)
        {
            if (FieldExists(jObject, "rings", JTokenType.Array))
            {
                return new PolygonContract();
            }
            else if (FieldExists(jObject, "paths", JTokenType.Array))
            {
                return new PolylineContract();
            }
            else if (FieldExists(jObject, "xmin", JTokenType.Float))
            {
                return new EnvelopeContract();
            }
            else if (FieldExists(jObject, "points", JTokenType.Array))
            {
                return new MultipointContract();
            }
            else if (FieldExists(jObject, "x", JTokenType.Float))
            {
                return new PointContract();
            }

            throw new InvalidOperationException("Could not determine geometry type.");
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
