using System;
using System.Runtime.Serialization;

namespace Wsdot.Elc.Contracts
{
	/// <summary>
	/// Provides information about a route.
	/// </summary>
	[DataContract]
	public class RouteInfo : IEquatable<RouteInfo>, IComparable<RouteInfo>, IComparable
	{
		private string _name;

		/// <summary>
		/// The name of the route
		/// </summary>
		[DataMember]
		public string Name
		{
			get { return _name; }
			set {
				if (value == null)
				{
					SR = Rrt = Rrq = null;
					HasValidName = false;
				}
				else
				{
					string sr, rrt, rrq;
					HasValidName = value.TryParse(out sr, out rrt, out rrq);
					_name = value;
					if (HasValidName)
					{
						SR = sr;
						Rrt = rrt;
						Rrq = rrq;
					}
					else
					{
						SR = null;
						Rrt = null;
						Rrq = null;
					}
				}
			}
		}

		/// <summary>
		/// The SR portion of a route name.
		/// </summary>
		[DataMember]
		public string SR { get; private set; }

		/// <summary>
		/// The Related Route Type portion of the route name.
		/// </summary>
		[DataMember(Name="RRT")]
		public string Rrt { get; private set; }

		/// <summary>
		/// The Related Route Qualifier portion of the route name.
		/// </summary>
		[DataMember(Name="RRQ")]
		public string Rrq { get; private set; }

        /// <summary>
        /// Indicates if <see cref="Name"/> is a valid state route name.
        /// </summary>
        public bool HasValidName { get; private set; }


		/// <summary>
		/// The different types of LRS layers in which the route appears.
		/// </summary>
		[DataMember]
		public LrsTypes LrsTypes { get; set; }

        /// <summary>
        /// The type of route (e.g., US, State Route, Interstate)
        /// </summary>
        [DataMember(IsRequired = false)]
        public RouteType RouteType { get; set; }

        /// <summary>
        /// Compares a <see cref="RouteInfo"/> with another <see cref="RouteInfo"/>.  Comparison is based on <see cref="Name"/> (case-insensitive) and 
        /// <see cref="LrsTypes"/>
        /// </summary>
        /// <param name="other">Another <see cref="RouteInfo"/></param>
        /// <returns>
        /// Returns <see langword="true"/> if both <see cref="RouteInfo"/> objects have the same <see cref="Name"/> and <see cref="LrsTypes"/>, 
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(RouteInfo other)
		{
			if (other == null)
			{
				return false;
			}
			if (LrsTypes != other.LrsTypes)
			{
				return false;
			}

			if ((Name == null && other.Name != null) || (Name != null && other.Name == null))
			{
				return false;
			}

			if (Name == null && other.Name == null)
			{
				return true;
			}
			else
			{
				return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase) == 0;
			}
		}

        /// <summary>
        /// Compares a <see cref="RouteInfo"/> with another <see cref="object"/>.  If <paramref name="obj"/> is another <see cref="RouteInfo"/>, 
        /// then comparison is based on <see cref="Name"/> (case-insensitive) and <see cref="LrsTypes"/>.  Otherwise <see langword="false"/> is returned.
        /// </summary>
        /// <param name="obj">Another object which may or may not be a <see cref="RouteInfo"/>.</param>
        /// <returns>
        /// Returns <see langword="false"/> if <paramref name="obj"/> is not a <see cref="RouteInfo"/> object.
        /// Returns <see langword="true"/> if both <see cref="RouteInfo"/> objects have the same <see cref="Name"/> and <see cref="LrsTypes"/>, 
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj)
		{
			RouteInfo other = obj as RouteInfo;
			if (other == null)
			{
				return false;
			}
			else
			{
				return Equals(other);
			}
		}

		/// <summary>
		/// Creates a string representation of the <see cref="RouteInfo"/> object.
		/// </summary>
		/// <returns>Returns a string representation of the <see cref="RouteInfo"/> object.</returns>
		public override string ToString()
		{
			return string.Format("{0}:{1}", Name ?? "Null", LrsTypes);
		}

		/// <summary>
		/// Returns a hash code for this object.
		/// </summary>
		/// <returns></returns>
		/// <seealso cref="object.GetHashCode()"/>
		public override int GetHashCode()
		{
			return Name != null ? Name.GetHashCode() : "NULL".GetHashCode() ^ LrsTypes.GetHashCode();
		}

        /// <summary>
        /// Compares this <see cref="RouteInfo"/> with another.  Comparison results are based first on the <see cref="Name"/>.
        /// If the <see cref="Name"/> values are the same then the <see cref="LrsTypes"/> are compared.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(RouteInfo other)
		{
			if (other == null)
			{
				return 1;
			}
			else if (Equals(other))
			{
				return 0;
			}

			if (Name != null && other.Name != null)
			{
				var comparison = string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
				if (comparison == 0)
				{
					// Names are the same.  Compare LrsTypes.
					return LrsTypes.CompareTo(other.LrsTypes);
				}
				else
				{
					return comparison;
				}
			}
			else if (Name == null)
			{
				return -1;
			}
			else
			{
				return 1;
			}
		}

        /// <summary>
        /// Compares the <see cref="RouteInfo"/> with another object cast as a <see cref="RouteInfo"/>.
        /// </summary>
        /// <param name="obj">Another object</param>
        /// <returns></returns>
        /// <seealso cref="CompareTo(RouteInfo)"/>
        public int CompareTo(object obj)
		{
			RouteInfo other = obj as RouteInfo;
			return CompareTo(other);
		}
	}
}
