using System.Runtime.Serialization;

namespace Wsdot.Elc.Contracts
{
    /// <summary>
    /// <see href="https://developers.arcgis.com/rest/services-reference/enterprise/map-service.htm"/>
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
