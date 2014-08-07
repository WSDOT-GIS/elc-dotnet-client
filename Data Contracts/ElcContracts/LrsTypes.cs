using System;

namespace Wsdot.Elc.Contracts
{
	/// <summary>
	/// Represents the different types of LRSes that a route can belong to.
	/// </summary>
	[Flags]
	public enum LrsTypes
	{
		/// <summary>No type specified</summary>
		None = 0,
		/// <summary>Route is in the increase LRS layer.</summary>
		Increase = 1,
		/// <summary>Route is in the decrease LRS layer.</summary>
		Decrease = 2,
		/// <summary>Route is in both the increase and decrease LRS layer.</summary>
		Both = Increase | Decrease,
		/// <summary>Route is in the ramp LRS layer.</summary>
		Ramp = 4
	}
}
