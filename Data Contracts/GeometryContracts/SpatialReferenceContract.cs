using System.Runtime.Serialization;

namespace Wsdot.Geometry.Contracts
{
	/// <summary>
	/// Represents a spatial reference system.
	/// </summary>
	[DataContract(Name = "SpatialReference")]
	public class SpatialReferenceContract
	{
		/// <summary>
		/// The well-known identifier (WKID) associated with the spatial reference system.
		/// </summary>
		[DataMember(Name = "wkid", EmitDefaultValue=false)]
		public int? Wkid { get; set; }

		/// <summary>
		/// The well-known text (WKT) associated with the spatial reference system.  This may be omitted if <see cref="SpatialReferenceContract.Wkid"/> is provided.
		/// </summary>
		[DataMember(Name = "wkt", EmitDefaultValue = false)]
		public string Wkt { get; set; }

		/// <summary>
		/// Returns the JSON representation of the spatial reference.
		/// </summary>
		/// <returns>JSON representation of the spatial reference.</returns>
		public override string ToString()
		{
			if (Wkid.HasValue)
			{
				return string.Format("{{\"wkid\":{0}}}", Wkid);
			}
			else
			{
				return string.Format("{{\"wkt\":\"{0}\"}}", Wkid);
			}
		}
	} 
}
