using System.Runtime.Serialization;

namespace Wsdot.Geometry.Contracts
{
    /// <summary>
    /// Represents a polyline geometry.
    /// </summary>
    [DataContract(Name = "Polyline")]
    public class PolylineContract : GeometryContract
    {
        /// <summary>
        /// The array of paths that make up a polyline.
        /// </summary>
        [DataMember(Name = "paths")]
        public double[][][] Paths { get; set; }

    }

}