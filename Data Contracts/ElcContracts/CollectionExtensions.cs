using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wsdot.Elc.Contracts
{
	using IDict = IDictionary<string, object>;
	using SRDict = Dictionary<string, Dictionary<string, List<RouteInfo>>>;
	using RrtDict = Dictionary<string, List<RouteInfo>>;

	/// <summary>
	/// Provides extension methods for collections.
	/// </summary>
	public static class CollectionExtensions
	{
		/// <summary>
		/// Adds route location data to a list of dictionaries.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dictList"></param>
		/// <param name="routeLocations"></param>
		public static void AddRouteLocationData<T>(this IEnumerable<T> dictList,
			IEnumerable<RouteLocation> routeLocations) where T : IDict
		{
			// Determines if the list of dictionaries and the list of route locations are the same length.
			bool sameLength = dictList.Count() == routeLocations.Count();
			for (int i = 0, l = dictList.Count(); i < l; i++)
			{
				// Get the corresponding route location for the current dictionary.
				// If dictList and routeLocations have differing lengths, the RouteLocation.Id property will be used
				// to determine the RouteLocation corresponding to the current dictionary.
				RouteLocation rl = sameLength ? routeLocations.ElementAt(i) : routeLocations.SingleOrDefault(r => r.Id == i);
				if (rl != null)
				{
					var dict = dictList.ElementAt(i);
					dict.AddRouteLocationData(rl);
				}
			}
		}

		/// <summary>
		/// Adds the properties of a <see cref="RouteLocation"/> to an <see cref="IDict"/>.
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="routeLocation"></param>
		public static void AddRouteLocationData(this IDict dictionary,
			RouteLocation routeLocation)
		{
			if (dictionary == null) throw new ArgumentNullException("dictionary");
			if (routeLocation == null) throw new ArgumentNullException("routeLocation");

			// TODO: do without reflection.
			var properties = from p in typeof(RouteLocation).GetProperties()
							 where !p.Name.EndsWith("AsString")
							 select new
							 {
								 Key = dictionary.CreateUniqueKey(p.Name),
								 Value = p.GetValue(routeLocation, null)
							 };
			foreach (var prop in properties)
			{
				dictionary.Add(prop.Key, prop.Value);
			}
		}

		/// <summary>
		/// Checks a dictionary for a specific key.  If that key is already in use, a number is added to the key
		/// to create a key that is not in use.
		/// </summary>
		/// <typeparam name="T">The type of objects contained in the dictionary as values.</typeparam>
		/// <param name="dictionary">An <see cref="IDictionary&lt;TKey,TValue&gt;"/> keyed by <see cref="string"/>.</param>
		/// <param name="keyCandidate">The key that will be checked in the dicitonary.</param>
		/// <returns></returns>
		public static string CreateUniqueKey<T>(this IDictionary<string, T> dictionary, string keyCandidate)
		{
			if (dictionary == null) throw new ArgumentNullException("dictionary");
			if (keyCandidate == null) throw new ArgumentNullException("keyCandidate");

			string output = keyCandidate;
			int i = 0;
			while (dictionary.ContainsKey(output))
			{
				output = string.Format("{0}{1}:", keyCandidate, i);
				i++;
			}
			return output;

		}

		/// <summary>
		/// Groups route infos by SR, then by RRT.  Useful for controls with separate SR, RRT, and RRQ drop-downs.
		/// </summary>
		/// <param name="routeInfo"></param>
		/// <returns></returns>
		public static SRDict GetSRRrtRrqDictionary(this IEnumerable<RouteInfo> routeInfo)
		{
			var output = new SRDict();

			foreach (var ri in routeInfo)
			{
				// If the name is invalid, skip to the next one.
				if (!ri.HasValidName) continue;

				// Get the dictionary of RRT values for the current SR, 
				// creating it if it does not already exist.
				RrtDict rrtDict;
				if (output.ContainsKey(ri.SR))
				{
					rrtDict = output[ri.SR];
				}
				else
				{
					rrtDict = new RrtDict();
					output.Add(ri.SR, rrtDict);
				}

				List<RouteInfo> rrqList;
				if (rrtDict.ContainsKey(ri.Rrt))
				{
					rrqList = rrtDict[ri.Rrt];
				}
				else
				{
					rrqList = new List<RouteInfo>();
					rrtDict.Add(ri.Rrt, rrqList);
				}

				rrqList.Add(ri);

			}

			return output;
		}

	}
}
