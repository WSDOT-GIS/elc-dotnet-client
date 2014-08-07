using System;
using System.Runtime.Serialization;

namespace Wsdot.Elc.Contracts
{
	/// <summary>
	/// Represents a set of Increase, Decrease, and Ramp locations that were located near the same user-specified point.
	/// </summary>
	[DataContract]
	public class RouteLocationSet
	{
		/// <summary>The nearest point in the Increase feature class.</summary>
		[DataMember(EmitDefaultValue=false)]
		public RouteLocation Increase { get; set; }
		
		/// <summary>The nearest point in the Decrease feature class.</summary>
		[DataMember(EmitDefaultValue=false)]
		public RouteLocation Decrease { get; set; }
		
		/// <summary>The nearest point in the Ramp feature class.</summary>
		[DataMember(EmitDefaultValue=false)]
		public RouteLocation Ramp { get; set; }

		/// <summary>
		/// Determines which <see cref="RouteLocation"/> has the shortest <see cref="RouteLocation.Distance"/> (based on absolute value) and returns
		/// that <see cref="RouteLocation"/>.
		/// </summary>
		/// <returns></returns>
		public RouteLocation ReturnLocationWithShortestOffset()
		{
			RouteLocation output = null;
			foreach (var loc in new RouteLocation[] { this.Increase, this.Decrease, this.Ramp })
			{
				if (loc != null)
				{
					if (output == null)
					{
						output = loc;
					}
					else
					{
						if (loc.Distance.HasValue && output.Distance.HasValue)
						{
							if (Math.Abs(loc.Distance.Value) < Math.Abs(output.Distance.Value))
							{
								output = loc;
							}
						}
						else if (loc.Distance.HasValue)
						{
							output = loc;
						}
					}
				}
			}

			return output;
		}
	}
}
