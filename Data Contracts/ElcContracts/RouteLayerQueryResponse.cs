using System.Collections.Generic;
using System.Runtime.Serialization;
using Wsdot.Geometry.Contracts;

namespace Wsdot.Elc.Contracts
{

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// <see href="http://resources.arcgis.com/en/help/arcgis-rest-api/index.html#/Query_Map_Service_Layer/02r3000000p1000000/"/>
    /// </summary>
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore IDE1006 // Naming Styles
}
