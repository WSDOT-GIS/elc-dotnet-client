using System.Runtime.Serialization;

namespace Wsdot.Geometry.Contracts
{
	/// <summary>
	/// An abstract class used as the basis for classes that represent geomtery.
	/// </summary>
	[DataContract(Name = "Geometry")]
	[KnownType(typeof(EnvelopeContract))]
	[KnownType(typeof(MultipointContract))]
	[KnownType(typeof(PointContract))]
	[KnownType(typeof(PolygonContract))]
	[KnownType(typeof(PolylineContract))]
	public abstract class GeometryContract
	{
		/// <summary>
		/// The spatial reference of the geometry.
		/// </summary>
		[DataMember(Name = "spatialReference", EmitDefaultValue=false)]
		public SpatialReferenceContract SpatialReference { get; set; }

		/////// <summary>
		/////// Overridden in decendant classes to return the ESRI SOAP equivalent geometry.
		/////// </summary>
		/////// <returns>Returns a <see cref="Geometry"/> object.</returns>
		////public abstract Geometry ToGeometry();
	} 
}
