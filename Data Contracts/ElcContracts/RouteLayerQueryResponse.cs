using System.Collections.Generic;
using System.Runtime.Serialization;
using Wsdot.Geometry.Contracts;

namespace Wsdot.Elc.Contracts
{
	[DataContract]
	public class RouteLayerQueryResponseFeature
	{
		[DataMember]
		public Dictionary<string, object> attributes { get; set; }

		[DataMember]
		public PolylineContract geometry { get; set; }
	}

	[DataContract]
	public class RouteLayerQueryResponse
	{
		[DataMember]
		public RouteLayerQueryResponseFeature[] features { get; set; }
	}
}
