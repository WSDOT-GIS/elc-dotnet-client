using System.Runtime.Serialization;

namespace Wsdot.Geometry.Contracts
{
    /// <summary>
    /// Represents a polygon
    /// </summary>
    [DataContract(Name = "Polygon")]
    public class PolygonContract : GeometryContract
    {

        /// <summary>
        /// The rings that make up the polygon.
        /// </summary>
        [DataMember(Name = "rings")]
        public double[][][] Rings { get; set; }

    } 
}
