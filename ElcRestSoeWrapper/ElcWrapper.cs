using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Wsdot.Elc.Contracts;
using Wsdot.Geometry.Contracts;

namespace Wsdot.Elc.Wrapper
{

	/// <summary>
	/// A .NET wrapper for the Enterprise Location Class REST Server Object Extension.
	/// </summary>
	public partial class ElcWrapper
	{
		const string _defaultUrl = "http://www.wsdot.wa.gov/geoservices/arcgis/rest/services/Shared/ElcRestSOE/MapServer/exts/ElcRestSoe";
		const string _mapServiceUrlRe = @"(?i)^(.+)/exts/.+$";
		const string _defaultFindRouteLocationOperationName = "Find Route Locations";
		const string _defaultFindNearestRouteLocationsOperationName = "Find Nearest Route Locations";
		const string _defaultRoutesResourceName = "routes";
		////private  Dictionary<string, Dictionary<string, LrsTypes>> _routes;
		private Dictionary<string, List<RouteInfo>> _routes;

		static private ElcWrapper _instance = null;

		/// <summary>
		/// Gets the URL of the ELC map service extension.
		/// </summary>
		public string Url { get; private set; }

		/// <summary>
		/// Gets the URL of the map service that is hosting the ELC REST SOE.
		/// </summary>
		public string MapServerUrl { get; private set; }

		/// <summary>
		/// Gets the name of the "Find Route Locations" operation.
		/// </summary>
		public string FindRouteLocationsOperationName { get; private set; }
		
		/// <summary>
		/// Gets the name of the "Find Nearest Route Locations" operation.
		/// </summary>
		public string FindNearestRouteLocationsOperationName { get; private set; }

		/// <summary>
		/// Gets the name of the "routes" resource.
		/// </summary>
		public string RoutesResoureName { get; private set; }


		/// <summary>
		/// Returns a dictionary.  Keys correspond to LRS years (group layer names in
		/// the map service).  Values are lists of <see cref="RouteInfo"/> objects.
		/// </summary>
		/// <remarks>
		/// The first time this property is accessed it will query the ELC REST SOE 
		/// service for the information, which will then be stored in memory.
		/// Subsequent requests will retrieve this information from memory.
		/// </remarks>
		/// <exception cref="WebException">Thrown if an error is encountered when requesting data.</exception>
		public Dictionary<string, List<RouteInfo>> Routes
		{
			get
			{
				if (_routes == null)
				{
					UriBuilder uriB = new UriBuilder(string.Format("{0}/{1}", this.Url, this.RoutesResoureName));
					uriB.Query = "f=json";
					var reqest = (HttpWebRequest)HttpWebRequest.Create(uriB.Uri);
					var response = (HttpWebResponse)reqest.GetResponse();
					var stream = response.GetResponseStream();

					string json;

					using (var reader = new StreamReader(stream))
					{
						json = reader.ReadToEnd();
					}

					_routes = JsonToRouteInfoDict(json);
				}
				return _routes;
			}
		}

		private  MapServerInfo _mapServerInfo;

		public MapServerInfo MapServerInfo
		{
			get
			{
				if (_mapServerInfo == null)
				{
					var url = this.MapServerUrl;
					url += "?f=json";

					string json;

					using (var c = new WebClient())
					{
						json = c.DownloadString(url);
					}

					_mapServerInfo = JsonConvert.DeserializeObject<MapServerInfo>(json, new JsonSerializerSettings
					{
						MissingMemberHandling = MissingMemberHandling.Ignore
					});

				}
				return _mapServerInfo;
			}
		}

		public LayerInfo[] Layers
		{
			get
			{
				return this.MapServerInfo == null ? null : this.MapServerInfo.Layers;
			}
		}

		/// <summary>
		/// Deserializes a JSON string into a dictionary of route information.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		private static Dictionary<string, List<RouteInfo>> JsonToRouteInfoDict(string json) {
			var intDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(json);

			return intDict.ToDictionary(
				kvp => kvp.Key,
				kvp => kvp.Value.Select(val => new RouteInfo
				{
					Name = val.Key,
					LrsTypes = (LrsTypes)val.Value
				}).ToList()
			);
		}

		/// <summary>
		/// Creates a new <see cref="ElcWrapper"/> object.
		/// </summary>
		/// <param name="url">The URL of the REST endpoint.  Defaults to the production URL.</param>
		/// <param name="findRouteLocationsOperationName">The name of the "Find Route Locations" operation.  You should normally be able to just use the default value.</param>
		/// <param name="findNearestRouteLocationsOperationName">The name of the "Find Nearest Route Locations" operation.  You should normally be able to just use the default value.</param>
		/// <param name="routesResourceName">The name of the "routes" resource.  You should normally be able to just use the default value.</param>
		protected ElcWrapper(string url=_defaultUrl, string findRouteLocationsOperationName=_defaultFindRouteLocationOperationName, string findNearestRouteLocationsOperationName=_defaultFindNearestRouteLocationsOperationName, string routesResourceName=_defaultRoutesResourceName)
		{
			this.Url = url;
			this.FindRouteLocationsOperationName = findRouteLocationsOperationName;
			this.FindNearestRouteLocationsOperationName = findNearestRouteLocationsOperationName;
			this.RoutesResoureName = routesResourceName;

			// Extract the map service URL from the ElcWrapper URL.
			var match = Regex.Match(this.Url, _mapServiceUrlRe);
			if (match.Success)
			{
				this.MapServerUrl = match.Groups[1].Value;
			}

		}

		/// <summary>
		/// Gets the single instance of the <see cref="ElcWrapper"/> object, creating it if it does not already exist.
		/// </summary>
		/// <param name="url">The URL of the REST endpoint.</param>
		/// <param name="findRouteLocationsOperationName">The name of the "Find Route Locations" operation.  You should normally be able to just use the default value.</param>
		/// <param name="findNearestRouteLocationsOperationName">The name of the "Find Nearest Route Locations" operation.  You should normally be able to just use the default value.</param>
		/// <returns>Returns an instance of the <see cref="ElcWrapper"/>.</returns>
		public static ElcWrapper GetInstance(string url = _defaultUrl, string findRouteLocationsOperationName = _defaultFindRouteLocationOperationName, string findNearestRouteLocationsOperationName = _defaultFindNearestRouteLocationsOperationName)
		{
			if (_instance == null)
			{
				_instance = new ElcWrapper(url, findRouteLocationsOperationName, findNearestRouteLocationsOperationName);
			}
			else
			{
				_instance.Url = url;
				_instance.FindRouteLocationsOperationName = findRouteLocationsOperationName;
				_instance.FindNearestRouteLocationsOperationName = findNearestRouteLocationsOperationName;
			}
			return _instance;
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
		/// Either a WKID <see cref="int"/> or a WKT <see cref="string"/> of the spatial reference to use with the output geometry.
		/// If omitted, the spatial reference of the LRS will be used.  (As of this writing, <see href="http://spatialreference.org/ref/epsg/2927/">2927</see>.)
		/// </param>
		/// <param name="lrsYear">A string indicating the LRS publication year.  If omitted, the current year's LRS will be used.</param>
		/// <returns>An array of <see cref="RouteLocation"/> objects.</returns>
		private RouteLocation[] FindRouteLocations(IEnumerable<RouteLocation> locations, DateTime? referenceDate, object outSR, string lrsYear)
		{

			WebRequest request;
			byte[] bytes;
			CreateFindRouteLocatoinsWebReqestAndParameterBytes(locations, referenceDate, outSR, lrsYear, out request, out bytes);


			Stream str = null;

			try
			{
				request.ContentLength = bytes.Length; // Count the bytes to be sent.
				str = request.GetRequestStream();
				str.Write(bytes, 0, bytes.Length);  // Send the request.
			}
			finally
			{
				if (str != null)
				{
					str.Close();
				}
			}

			string jsonResults;

			// Get the response.
			WebResponse response = request.GetResponse();
			if (response != null)
			{
				using (var streamReader = new StreamReader(response.GetResponseStream()))
				{
					jsonResults = streamReader.ReadToEnd();
				}
			}
			else
			{
				return null;
			}

			// Deserialize the response
			return jsonResults.ToRouteLocations<RouteLocation[]>();
		}

		private void CreateFindRouteLocatoinsWebReqestAndParameterBytes(IEnumerable<RouteLocation> locations, DateTime? referenceDate, 
			object outSR, string lrsYear, out WebRequest request, out byte[] bytes)
		{
			// Build the query string.
			var parameters = new Dictionary<string, string>();
			parameters.Add("f", "json");
			parameters.Add("locations", locations.ToJson());
			parameters.Add("referenceDate", referenceDate.HasValue ? referenceDate.Value.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern) : string.Empty);
			if (outSR != null)
			{
				parameters.Add("outSR", outSR.ToString());
			}
			if (!string.IsNullOrEmpty(lrsYear))
			{
				parameters.Add("lrsYear", lrsYear);
			}

			var builder = new UriBuilder(this.Url + "/" + this.FindRouteLocationsOperationName);

			request = WebRequest.Create(builder.Uri);
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";
			bytes = parameters.ToQueryStringBytes();
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
		/// <returns>An array of <see cref="RouteLocation"/> objects.</returns>
		public RouteLocation[] FindRouteLocations(IEnumerable<RouteLocation> locations, DateTime? referenceDate, int? outSR, string lrsYear)
		{
			return FindRouteLocations(locations, referenceDate, outSR as object, lrsYear);
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
		/// <returns>An array of <see cref="RouteLocation"/> objects.</returns>
		public RouteLocation[] FindRouteLocations(IEnumerable<RouteLocation> locations, DateTime? referenceDate, string outSR, string lrsYear)
		{
			return FindRouteLocations(locations, referenceDate, outSR as object, lrsYear);
		}



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
		/// <returns>An array of <see cref="RouteLocation"/> objects.</returns>
		public RouteLocation[] FindNearestRouteLocations(IEnumerable<double> coordinates, DateTime referenceDate, double searchRadius, 
			object inSR, object outSR, string lrsYear, string routeFilter = null)
		{
			WebRequest request;
			byte[] bytes;
			CreateFindNearestRouteLocationsWebRequest(coordinates, referenceDate, searchRadius, inSR, outSR, lrsYear, routeFilter, out request, out bytes);


			Stream str = null;

			try
			{
				str = request.GetRequestStream();
				str.Write(bytes, 0, bytes.Length);  // Send the request.
			}
			finally
			{
				if (str != null)
				{
					str.Close();
				}
			}

			string jsonResults = null;

			// Get the response.
			using (WebResponse response = request.GetResponse())
			{
				if (response != null)
				{
					using (var streamReader = new StreamReader(response.GetResponseStream()))
					{
						jsonResults = streamReader.ReadToEnd();
					}
				}
			}

			// Deserialize the response
			return jsonResults == null ? null : jsonResults.ToRouteLocations<RouteLocation[]>();
		}

		private void CreateFindNearestRouteLocationsWebRequest(IEnumerable<double> coordinates, DateTime referenceDate, double searchRadius, object inSR, object outSR, string lrsYear, string routeFilter, out WebRequest request, out byte[] bytes)
		{
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			// Build the query string.
			var parameters = new Dictionary<string, string>();
			parameters.Add("f", "json");
			parameters.Add("coordinates", serializer.Serialize(coordinates));
			parameters.Add("referenceDate", referenceDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern));
			parameters.Add("searchRadius", searchRadius.ToString());
			if (inSR != null)
			{
				parameters.Add("inSR", inSR.ToString());
			}
			if (outSR != null)
			{
				parameters.Add("outSR", outSR.ToString());
			}
			if (!string.IsNullOrEmpty(lrsYear))
			{
				parameters.Add("lrsYear", lrsYear);
			}
			if (!string.IsNullOrEmpty(routeFilter))
			{
				parameters.Add("routeFilter", routeFilter);
			}

			UriBuilder builder = new UriBuilder(this.Url + "/" + this.FindNearestRouteLocationsOperationName);

			request = WebRequest.Create(builder.Uri);
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";
			bytes = parameters.ToQueryStringBytes();
			request.ContentLength = bytes.Length;
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
		public RouteLocation[] FindNearestRouteLocations<T>(IEnumerable<T> coordinates, DateTime referenceDate, double searchRadius,
			object inSR, object outSR, string lrsYear, string routeFilter = null) where T : class, IEnumerable<double>
		{
			return FindNearestRouteLocations(coordinates.SelectMany(c => c.Take(2)), referenceDate, searchRadius, inSR, outSR, lrsYear, routeFilter);
		}

		public Dictionary<LrsTypes, PolylineContract> FindRoute(RouteInfo routeInfo, string LrsYear="Current", int? outSR=null)
		{
			var layers = this.Layers;
			LayerInfo parentLayer = string.IsNullOrEmpty(LrsYear) ? layers.First(l => l.ParentLayerId == -1) : layers.First(l => l.Name == LrsYear);
			IEnumerable<LayerInfo> subLayers = layers.Where(l => l.ParentLayerId == parentLayer.Id);
			Dictionary<LrsTypes, PolylineContract> output = new Dictionary<LrsTypes,PolylineContract>(2);

			/*
			 * http://wsdot.wa.gov/geosvcs/ArcGIS/rest/services/Shared/ElcRestSoe/MapServer/1/query?text=&where=RouteID+%3D+%27005%27&returnGeometry=true&maxAllowableOffset=&outSR=&f=json
			 */

			// Create the format string for the web requests.
			string fmt = string.Format("{0}/{{0}}/query?where=RouteID+%3D+%27{{1}}%27&returnGeometry=true&outSR={1}&f=json", 
				this.MapServerUrl, outSR.HasValue ? outSR.Value.ToString() : string.Empty);

			string iJson = null, dJson = null, rJson = null;

			using (var client = new WebClient())
			{
				string url;
				LayerInfo layerInfo;

				if ((routeInfo.LrsTypes & LrsTypes.Increase) == LrsTypes.Increase)
				{
					layerInfo = subLayers.FirstOrDefault(l => string.Compare(l.Name, "Increase", StringComparison.InvariantCultureIgnoreCase) == 0);
					if (layerInfo != null)
					{
						url = string.Format(fmt, layerInfo.Id, routeInfo.Name);
						iJson = client.DownloadString(url);
					}
				}
				if ((routeInfo.LrsTypes & LrsTypes.Decrease) == LrsTypes.Decrease)
				{
					layerInfo = subLayers.FirstOrDefault(l => string.Compare(l.Name, "Decrease", StringComparison.InvariantCultureIgnoreCase) == 0);
					if (layerInfo != null)
					{
						url = string.Format(fmt, layerInfo.Id, routeInfo.Name);
						dJson = client.DownloadString(url);
					}
				}
				if ((routeInfo.LrsTypes & LrsTypes.Ramp) == LrsTypes.Ramp)
				{
					layerInfo = subLayers.FirstOrDefault(l => string.Compare(l.Name, "Ramp", StringComparison.InvariantCultureIgnoreCase) == 0);
					if (layerInfo != null)
					{
						url = string.Format(fmt, layerInfo.Id, routeInfo.Name);
						rJson = client.DownloadString(url);
					}
				}
			}

			RouteLayerQueryResponse response;
			RouteLayerQueryResponseFeature feature;
			var jsSettings = new JsonSerializerSettings
			{
				MissingMemberHandling = MissingMemberHandling.Ignore
			};
			if (!string.IsNullOrEmpty(iJson))
			{
				response = JsonConvert.DeserializeObject<RouteLayerQueryResponse>(iJson, jsSettings);
				feature = response.features.FirstOrDefault();
				if (feature != null) {
					output.Add(LrsTypes.Increase, feature.geometry as PolylineContract);
				}
			}
			if (!string.IsNullOrEmpty(dJson))
			{
				response = JsonConvert.DeserializeObject<RouteLayerQueryResponse>(dJson, jsSettings);
				feature = response.features.FirstOrDefault();
				if (feature != null)
				{
					output.Add(LrsTypes.Decrease, feature.geometry as PolylineContract);
				}
			}
			if (!string.IsNullOrEmpty(rJson))
			{
				response = JsonConvert.DeserializeObject<RouteLayerQueryResponse>(rJson, jsSettings);
				feature = response.features.FirstOrDefault();
				if (feature != null)
				{
					output.Add(LrsTypes.Ramp, feature.geometry as PolylineContract);
				}
			}

			if (output.Count == 0)
			{
				output = null;
			}

			return output;
		}
	}
}
