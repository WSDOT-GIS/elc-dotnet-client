using System.Runtime.Serialization;

namespace Wsdot.Elc.Contracts
{
    /// <summary>
    /// <see href="http://resources.arcgis.com/en/help/arcgis-rest-api/index.html#/Map_Service/02r3000000w2000000/"/>
    /// </summary>
    [DataContract]
    public class MapServerInfo
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        [DataMember(Name="layers")]
        public LayerInfo[] Layers { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
