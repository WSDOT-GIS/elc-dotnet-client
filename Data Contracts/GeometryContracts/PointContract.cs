using System.Runtime.Serialization;

namespace Wsdot.Geometry.Contracts
{
	/// <summary>
	/// Represents a point
	/// </summary>
	[DataContract(Name = "Point")]
	public class PointContract : GeometryContract
	{

		/// <summary>The X coordinate of the point.</summary>
		[DataMember(Name = "x")]
		public double X { get; set; }

		/// <summary>The Y coordinate of the point.</summary>
		[DataMember(Name = "y")]
		public double Y { get; set; }

		/// <summary>
		/// Returns the JSON representation of this point.
		/// </summary>
		/// <returns>Returns the JSON representation of this point.</returns>
		public override string ToString()
		{
			if (this.SpatialReference != null)
			{
				return string.Format("{{ \"x\":{0}, \"y\":{1}, \"spatialReference\": {2} }}", this.X, this.Y, this.SpatialReference);
			}
			else
			{
				return string.Format("{{ \"x\":{0}, \"y\":{1} }}", this.X, this.Y);

			}
		}

		/// <summary>
		/// Converts point to an array of doubles.
		/// </summary>
		/// <returns></returns>
		public double[] ToArray()
		{
			return new double[] { this.X, this.Y };
		}
	} 
}
