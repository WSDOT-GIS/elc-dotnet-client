using System.Runtime.Serialization;

namespace Wsdot.Geometry.Contracts
{
    /// <summary>
    /// Represents a Multipoint.
    /// </summary>
    [DataContract(Name = "Multipoint")]
    public class MultipointContract : GeometryContract
    {
        /// <summary>
        /// The points that make up the multipoint.
        /// </summary>
        [DataMember(Name = "points")]
        public double[][] Points { get; set; }
    } 
}
