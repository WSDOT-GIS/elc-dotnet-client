
using System.Runtime.Serialization;

namespace Wsdot.Geometry.Contracts
{
    /// <summary>
    /// Represents an envelope (a.k.a, extent).
    /// </summary>
    [DataContract(Name = "Envelope")]
    public class EnvelopeContract : GeometryContract
    {
        /// <summary>The lower-left X coordinate.</summary>
        [DataMember(Name = "xmin")]
        public double XMin { get; set; }

        /// <summary>The lower-left Y coordinate.</summary>
        [DataMember(Name = "ymin")]
        public double YMin { get; set; }

        /// <summary>The upper-right X coordinate.</summary>    
        [DataMember(Name = "xmax")]
        public double XMax { get; set; }

        /// <summary>The upper-right Y coordinate.</summary>    
        [DataMember(Name = "ymax")]
        public double YMax { get; set; }
    } 
}
