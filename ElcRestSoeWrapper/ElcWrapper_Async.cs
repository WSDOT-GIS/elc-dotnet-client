using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
		public void GetRoutesAsynch(Action<Dictionary<string, List<RouteInfo>>> action, Action<Exception> errorAction)
		{
			// Check to see if the list of routes has already been retrieved.
			// If it has, just invoke "action" on the existing list of routes.
			// Otherwise begin the web request to retrieve the list.
			if (_routes == null)
			{
				// Create the URI for the web request.
				UriBuilder uriB = new UriBuilder(string.Format("{0}/{1}", this.Url, this.RoutesResoureName));
				uriB.Query = "f=json";
				// Create the web request using the URI.
				var request = (HttpWebRequest)HttpWebRequest.Create(uriB.Uri);

				// Create a dictionary containing the web request and the function that will be called when the request is complete.
				var dict = new Dictionary<string, object>();
				dict["request"] = request;
				dict["action"] = action;
				dict["errorAction"] = errorAction;
				// Initiate the web request.
				IAsyncResult result = request.BeginGetResponse(new AsyncCallback(OnGetRoutesComplete), dict);
			}
			else if (action != null)
			{
				action.Invoke(_routes);
			}

		}

		/// <summary>
		/// This method handles the response from the web request initiated by <see cref="GetRoutesAsynch"/>.
		/// </summary>
		/// <param name="result"></param>
		private void OnGetRoutesComplete(IAsyncResult result)
		{
			Action<Exception> errorAction = null;
			try
			{
				// Get the web request and the function to be called on the result.
				var dict = result.AsyncState as Dictionary<string, object>;
				var request = (HttpWebRequest)dict["request"];
				var action = dict["action"] as Action<Dictionary<string, List<RouteInfo>>>;
				errorAction = dict["errorAction"] as Action<Exception>;

				// Get the response.
				HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

				// Get the response stream.
				var stream = response.GetResponseStream();

				// Read the response stream into a JSON string.
				string json;
				using (var reader = new StreamReader(stream))
				{
					json = reader.ReadToEnd();
				}

				// Deserialize the JSON string into a dictionary of routes.
				_routes = JsonToRouteInfoDict(json);

				if (action != null)
				{
					action.Invoke(_routes);
				}
			}
			catch (Exception ex)
			{
				if (errorAction != null)
				{
					errorAction.Invoke(ex);
				}
				else
				{
					throw;
				}
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
		/// Either a WKID <see cref="int"/> or a WKT <see cref="string"/> of the spatial reference to use with the output geometry.
		/// If omitted, the spatial reference of the LRS will be used.  (As of this writing, <see href="http://spatialreference.org/ref/epsg/2927/">2927</see>.)
		/// </param>
		/// <param name="lrsYear">A string indicating the LRS publication year.  If omitted, the current year's LRS will be used.</param>
		/// <param name="action">The action that occurs when the operation is complete.</param>
		/// <param name="errorAction">The action that occurs when an <see cref="Exception"/> is encountered.</param>
		private void FindRouteLocationsAsync(IEnumerable<RouteLocation> locations, DateTime? referenceDate, object outSR,
			string lrsYear, Action<RouteLocation[]> action, Action<Exception> errorAction)
		{
			WebRequest request;
			byte[] bytes;
			CreateFindRouteLocatoinsWebReqestAndParameterBytes(locations, referenceDate, outSR, lrsYear, out request, out bytes);

			request.ContentLength = bytes.Length;

			var dict = new Dictionary<string, object>();
			dict["request"] = request;
			dict["bytes"] = bytes;
			dict["action"] = action;
			dict["errorAction"] = errorAction;

			request.BeginGetRequestStream(new AsyncCallback(OnGetFindRouteLocationsRequestStream), dict);
		}

		private void OnGetFindRouteLocationsRequestStream(IAsyncResult result)
		{
			var dict = (Dictionary<string, object>)result.AsyncState;
			var request = (HttpWebRequest)dict["request"];
			var bytes = (byte[])dict["bytes"];

			Stream requestStream = null;
			try
			{
				requestStream = request.EndGetRequestStream(result);
				requestStream.Write(bytes, 0, bytes.Length);
			}
			finally
			{
				if (requestStream != null)
				{
					requestStream.Close();
				}
			}

			dict.Remove("bytes");
			request.BeginGetResponse(new AsyncCallback(OnGetFindRouteLocationsResponse), dict);
		}

		private void OnGetFindRouteLocationsResponse(IAsyncResult result)
		{
			RouteLocation[] locations = null;
			var dict = (Dictionary<string, object>)result.AsyncState;
			var action = dict["action"] as Action<RouteLocation[]>;
			var errorAction = dict["errorAction"] as Action<Exception>;
			try
			{
				var request = (HttpWebRequest)dict["request"];

				var response = (HttpWebResponse)request.EndGetResponse(result);

				string json;
				using (var streamReader = new StreamReader(response.GetResponseStream()))
				{
					json = streamReader.ReadToEnd();
				}

				locations = json.ToRouteLocations<RouteLocation[]>();
			}
			catch (Exception ex)
			{
				if (errorAction != null)
				{
					errorAction.Invoke(ex);
				}
				else
				{
					throw;
				}
			}

			if (action != null)
			{
				action.Invoke(locations);
			}
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
		/// The WKID <see cref="int"/> of the spatial reference to use with the output geometry.
		/// If omitted, the spatial reference of the LRS will be used.  (As of this writing, <see href="http://spatialreference.org/ref/epsg/2927/">2927</see>.)
		/// </param>
		/// <param name="lrsYear">A string indicating the LRS publication year.  If omitted, the current year's LRS will be used.</param>
		/// <param name="action">The action that occurs when the operation is complete.</param>
		/// <param name="errorAction">The action that occurs when an <see cref="Exception"/> is encountered.</param>
		public void FindRouteLocationsAsync(IEnumerable<RouteLocation> locations, DateTime? referenceDate, int? outSR,
			string lrsYear, Action<RouteLocation[]> action, Action<Exception> errorAction)
		{
			FindRouteLocationsAsync(locations, referenceDate, outSR as object, lrsYear, action, errorAction);
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
		public void FindRouteLocationsAsync(IEnumerable<RouteLocation> locations, DateTime? referenceDate, string outSR,
			string lrsYear, Action<RouteLocation[]> action, Action<Exception> errorAction)
		{
			FindRouteLocationsAsync(locations, referenceDate, outSR as object, lrsYear, action, errorAction);
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
		public void FindNearestRouteLocationsAsync(IEnumerable<double> coordinates, DateTime referenceDate, 
			double searchRadius, object inSR, object outSR, string lrsYear, 
			Action<RouteLocation[]> action, Action<Exception> failAction, string routeFilter = null)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			WebRequest request;
			byte[] bytes;
			CreateFindNearestRouteLocationsWebRequest(coordinates, referenceDate, searchRadius, inSR, outSR, 
				lrsYear, routeFilter, out request, out bytes);

			var dict = new Dictionary<string, object>();
			dict["request"] = request;
			dict["bytes"] = bytes;
			dict["action"] = action;
			dict["failAction"] = failAction;

			request.BeginGetRequestStream(new AsyncCallback(OnGetFindNearestRouteLocationsRequestStream), dict);
		}

		private void OnGetFindNearestRouteLocationsRequestStream(IAsyncResult result)
		{
			var dict = (Dictionary<string, object>)result.AsyncState;
			var request = (HttpWebRequest)dict["request"];
			var bytes = (byte[])dict["bytes"];
			var action = dict["action"] as Action<RouteLocation[]>;
			var failAction = dict["failAction"] as Action<Exception>;
			Stream stream = null;
			try
			{
				stream = request.EndGetRequestStream(result);
				stream.Write(bytes, 0, bytes.Length);  // Send the request.
				request.BeginGetResponse(new AsyncCallback(OnGetFindNearestRouteLocationsResponse), dict);
			}
			catch (Exception ex)
			{
				if (failAction != null)
				{
					failAction.Invoke(ex);
				}
				else
				{
					throw;
				}
			}
			finally
			{
				if (stream != null) stream.Close();
			}

		}

		private void OnGetFindNearestRouteLocationsResponse(IAsyncResult result)
		{
			var dict = (Dictionary<string, object>)result.AsyncState;
			var request = (HttpWebRequest)dict["request"];
			var action = dict["action"] as Action<RouteLocation[]>;
			var failAction = dict["failAction"] as Action<Exception>;

			try
			{
				string jsonResults = null;
				using (var response = request.EndGetResponse(result))
				{

					// Get the response.
					if (response != null)
					{
						using (var streamReader = new StreamReader(response.GetResponseStream()))
						{
							jsonResults = streamReader.ReadToEnd();
						}
					}
				}

				var routeLocations = jsonResults == null ? null : jsonResults.ToRouteLocations<RouteLocation[]>();
				if (action != null)
				{
					action.Invoke(routeLocations);
				}
			}
			catch (Exception ex)
			{
				if (failAction != null)
				{
					failAction.Invoke(ex);
				}
				else
				{
					throw;
				}
			}
			
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
		public void FindNearestRouteLocationsAsync<T>(IEnumerable<T> coordinates, DateTime referenceDate,
			double searchRadius, object inSR, object outSR, string lrsYear,
			Action<RouteLocation[]> action, Action<Exception> failAction, string routeFilter = null) where T : class, IEnumerable<double>
		{
			FindNearestRouteLocationsAsync(coordinates.SelectMany(c => c.Take(2)), referenceDate, searchRadius, inSR, outSR, lrsYear, action, failAction, routeFilter);
		}

		#endregion
	}
}
