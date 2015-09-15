using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Wsdot.Geometry.Contracts;

namespace Wsdot.Elc.Serialization
{
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
			return typeof(T).IsAssignableFrom(objectType);
		}



		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
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
			JToken token;
			return jObject.TryGetValue(name, out token) && token.Type == type;
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
}
