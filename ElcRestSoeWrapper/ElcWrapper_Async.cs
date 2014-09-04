using System;
using System.Collections.Generic;
using System.Linq;
using Wsdot.Elc.Contracts;

namespace Wsdot.Elc.Wrapper
{

	public partial class ElcWrapper
	{
		#region Get Routes
		/// <summary>
		/// Begins an asynchronous operation to retrieve a list of routes.
		/// </summary>
		/// <param name="action">
		/// This is the function that will be called when the web request is completed.
		/// It takes a single parameter, a <see cref="Dictionary&lt;TKey,TValue&gt;"/> dictionary keyed by LRS year strings and its values are lists of <see cref="RouteInfo"/>.
		/// </param>
		/// <param name="errorAction">This is the action that occurs when an <see cref="Exception"/> is encountered.</param>
		/// <remarks>After the first time the web request has been made, subsequent calls return the cached list of routes.</remarks>
		[Obsolete("Use GetRoutesAsync() instead.")]
		public void GetRoutesAsynch(Action<Dictionary<string, List<RouteInfo>>> action, Action<Exception> errorAction)
		{
			// Check to see if the list of routes has already been retrieved.
			// If it has, just invoke "action" on the existing list of routes.
			// Otherwise begin the web request to retrieve the list.
			if (_routes == null)
			{
				GetRouteDict().ContinueWith(t => _routes = t.Result).Wait();

				if (action != null)
				{
					action.Invoke(_routes);
				}
			}
			else if (action != null)
			{
				action.Invoke(_routes);
			}

		}
		#endregion

		#region Get Route Locations

		/// <summary>
		/// Finds route locations on the WSDOT Linear Referencing System (LRS).
		/// </summary>
		/// <param name="locations">A collection of <see cref="RouteLocation"/>s.</param>
		/// <param name="referenceDate">
		/// The date that the <paramref name="locations"/> were collected.  
		/// This parameter can be omitted if EACH item in <paramref name="locations"/>
		/// has a value for its <see cref="RouteLocation.ReferenceDate"/>.
		/// </param>
		/// <param name="outSR">
		/// The WKID <see cref="int"/> of the spatial reference to use with the output geometry.
		/// If omitted, the spatial reference of the LRS will be used.  (As of this writing, <see href="http://spatialreference.org/ref/epsg/2927/">2927</see>.)
		/// </param>
		/// <param name="lrsYear">A string indicating the LRS publication year.  If omitted, the current year's LRS will be used.</param>
		/// <param name="action">The action that occurs when the operation is complete.</param>
		/// <param name="errorAction">The action that occurs when an <see cref="Exception"/> is encountered.</param>
		[Obsolete("Use the async alternative instead.")]
		public void FindRouteLocationsAsync(IEnumerable<RouteLocation> locations, DateTime? referenceDate, int? outSR,
			string lrsYear, Action<RouteLocation[]> action, Action<Exception> errorAction)
		{
			FindRouteLocationsAsync(locations, referenceDate, outSR as object, lrsYear).ContinueWith(t =>
			{
				if (t.Exception != null && errorAction != null)
				{
					errorAction.Invoke(t.Exception);
				}
				if (action != null)
				{
					action.Invoke(t.Result);
				}
			});
		}



		/// <summary>
		/// Finds route locations on the WSDOT Linear Referencing System (LRS).
		/// </summary>
		/// <param name="locations">A collection of <see cref="RouteLocation"/>s.</param>
		/// <param name="referenceDate">
		/// The date that the <paramref name="locations"/> were collected.  
		/// This parameter can be omitted if EACH item in <paramref name="locations"/>
		/// has a value for its <see cref="RouteLocation.ReferenceDate"/>.
		/// </param>
		/// <param name="outSR">
		/// The a WKT <see cref="string"/> of the spatial reference to use with the output geometry.
		/// If omitted, the spatial reference of the LRS will be used.  (As of this writing, <see href="http://spatialreference.org/ref/epsg/2927/">2927</see>.)
		/// </param>
		/// <param name="lrsYear">A string indicating the LRS publication year.  If omitted, the current year's LRS will be used.</param>
		/// <param name="action">The action that occurs when the operation is complete.</param>
		/// <param name="errorAction">The action that occurs when an <see cref="Exception"/> is encountered.</param>
		[Obsolete("Use the async alternative instead.")]
		public void FindRouteLocationsAsync(IEnumerable<RouteLocation> locations, DateTime? referenceDate, string outSR,
			string lrsYear, Action<RouteLocation[]> action, Action<Exception> errorAction)
		{
			FindRouteLocationsAsync(locations, referenceDate, outSR as object, lrsYear).ContinueWith(t =>
			{
				if (t.Exception != null && errorAction != null)
				{
					errorAction.Invoke(t.Exception);
				}
				if (action != null)
				{
					action.Invoke(t.Result);
				}
			});
		}

		#endregion

		#region Get Nearest Route Locations
		/// <summary>
		/// Returns route locations closest to a given set of point coordinates.
		/// </summary>
		/// <param name="coordinates">
		/// An array of <see cref="double"/> values.  Values at even indexes are X coordinates, odd are Y coordinates.
		/// I.e., the first point is represented by items 0 and 1, the second by 2 and 3, etc.
		/// </param>
		/// <param name="referenceDate">The date that the <paramref name="coordinates"/> were collected.</param>
		/// <param name="searchRadius">The maximum distance in feet around each of the <paramref name="coordinates"/> to search for a route.</param>
		/// <param name="inSR">
		/// The spatial reference corresponding to <paramref name="coordinates"/>.  This value should be either a WKID <see cref="int"/> or a WKT <see cref="string"/>.
		/// </param>
		/// <param name="outSR">
		/// The spatial reference system to use in the output <see cref="RouteLocation"/> objects.
		/// This value should be either a WKID <see cref="int"/> or a WKT <see cref="string"/>.
		/// </param>
		/// <param name="lrsYear">Specifies which LRS to use.</param>
		/// <param name="routeFilter">
		/// A partial SQL query that can be used to limit which routes are searched.  This value is optional.
		/// <example>
		/// <list type="bullet">
		/// <item><description><c>LIKE '005%'</c></description></item>
		/// <item><description><c>= '005'</c></description></item>
		/// </list>
		/// </example>
		/// </param>
		/// <param name="action">The action that is taken when the service request gets a sucessfull response.</param>
		/// <param name="failAction">The action that is taken when the service request fails.</param>
		[Obsolete("Use the async alternative instead.")]
		public void FindNearestRouteLocationsAsync(IEnumerable<double> coordinates, DateTime referenceDate, 
			double searchRadius, object inSR, object outSR, string lrsYear, 
			Action<RouteLocation[]> action, Action<Exception> failAction, string routeFilter = null)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}

			FindNearestRouteLocationsAsync(coordinates, referenceDate, searchRadius, inSR, outSR, lrsYear, routeFilter).ContinueWith(t => {
				if (t.Exception != null && failAction != null)
				{
					failAction.Invoke(t.Exception);
				}
				action.Invoke(t.Result);
			});
		}

		/// <summary>
		/// Returns route locations closest to a given set of point coordinates.
		/// </summary>
		/// <typeparam name="T">A <see langword="class"/> that implements <see cref="IEnumerable&lt;T&gt;"/> of <see cref="double"/>.</typeparam>
		/// <param name="coordinates">A jagged array of <see cref="double"/> values representing points.</param>
		/// <param name="referenceDate">The date that the <paramref name="coordinates"/> were collected.</param>
		/// <param name="searchRadius">The maximum distance in feet around each of the <paramref name="coordinates"/> to search for a route.</param>
		/// <param name="inSR">
		/// The spatial reference corresponding to <paramref name="coordinates"/>.  This value should be either a WKID <see cref="int"/> or a WKT <see cref="string"/>.
		/// </param>
		/// <param name="outSR">
		/// The spatial reference system to use in the output <see cref="RouteLocation"/> objects.
		/// This value should be either a WKID <see cref="int"/> or a WKT <see cref="string"/>.
		/// </param>
		/// <param name="lrsYear">Specifies which LRS to use.</param>
		/// <param name="routeFilter">
		/// A partial SQL query that can be used to limit which routes are searched.  This value is optional.
		/// <example>
		/// <list type="bullet">
		/// <item><description><c>LIKE '005%'</c></description></item>
		/// <item><description><c>= '005'</c></description></item>
		/// </list>
		/// </example>
		/// </param>
		/// <returns>An array of <see cref="RouteLocation"/> objects.</returns>
		/// <param name="action">The action that is taken when the service request gets a sucessfull response.</param>
		/// <param name="failAction">The action that is taken when the service request fails.</param>
		[Obsolete("Use the async alternative instead.")]
		public void FindNearestRouteLocationsAsync<T>(IEnumerable<T> coordinates, DateTime referenceDate,
			double searchRadius, object inSR, object outSR, string lrsYear,
			Action<RouteLocation[]> action, Action<Exception> failAction, string routeFilter = null) where T : class, IEnumerable<double>
		{
			FindNearestRouteLocationsAsync(coordinates.SelectMany(c => c.Take(2)), referenceDate, searchRadius, inSR, outSR, lrsYear, action, failAction, routeFilter);
		}

		#endregion
	}
}
