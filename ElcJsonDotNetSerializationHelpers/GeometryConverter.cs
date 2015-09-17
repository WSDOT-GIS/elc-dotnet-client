using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Wsdot.Geometry.Contracts;

namespace Wsdot.Elc.Serialization
{
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
