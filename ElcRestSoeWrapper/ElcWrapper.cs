using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wsdot.Elc.Contracts;
using Wsdot.Geometry.Contracts;

namespace Wsdot.Elc.Wrapper
{

	/// <summary>
	/// A .NET wrapper for the Enterprise Location Class REST Server Object Extension.
	/// </summary>
	public partial class ElcWrapper
	{
		const string _defaultUrl = "https://data.wsdot.wa.gov/arcgis/rest/services/Shared/ElcRestSOE/MapServer/exts/ElcRestSoe";
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
					GetRoutesAsync().Wait();
				}
				return _routes;
			}
		}

		/// <summary>
		/// Asynchonous alternative to <see cref="ElcWrapper.Routes"/>.
		/// </summary>
		/// <returns></returns>
		public async Task<Dictionary<string, List<RouteInfo>>> GetRoutesAsync()
		{
			if (_routes == null)
			{

				return await GetRouteDict().ContinueWith(t => _routes = t.Result);
			}
			else
			{
				return _routes;
			}
		}

		private async Task<Dictionary<string, List<RouteInfo>>> GetRouteDict()
		{
			UriBuilder uriB = new UriBuilder(string.Format("{0}/{1}", this.Url, this.RoutesResoureName));
			uriB.Query = "f=json";

			using (var httpClient = new HttpClient())
			using (var stream = await httpClient.GetStreamAsync(uriB.Uri))
			{
				return await JsonToRouteInfoDict(stream);
			}
		}

		private  MapServerInfo _mapServerInfo;

		/// <summary>
		/// Information about the ELC Map Service.
		/// </summary>
		public MapServerInfo MapServerInfo
		{
			get
			{
				if (_mapServerInfo == null)
				{
					GetMapServerInfoAsync().Wait();
				}
				return _mapServerInfo;
			}
		}

		/// <summary>
		/// Gets information about the ELC map service.
		/// </summary>
		/// <returns>Returns the information about the map service.</returns>
		/// <remarks>
		/// An HTTP request will only be made the first time this function is called. 
		/// The response is cached for all future requests for the lifetime of the <see cref="ElcWrapper"/>.
		/// </remarks>
		public async Task<MapServerInfo> GetMapServerInfoAsync()
		{
			if (_mapServerInfo == null)
			{
				var url = this.MapServerUrl;
				url += "?f=json";

				using (var c = new HttpClient())
				using (var stream = await c.GetStreamAsync(url))
				using (var sr = new StreamReader(stream))
				using (var jr = new JsonTextReader(sr))
				{
					var js = JsonSerializer.Create(new JsonSerializerSettings
					{
						MissingMemberHandling = MissingMemberHandling.Ignore
					});
					_mapServerInfo = await Task.Run(() =>
					{
						return js.Deserialize<MapServerInfo>(jr);
					});
				}
			}
			return _mapServerInfo;
		}

		/// <summary>
		/// An array of <see cref="LayerInfo"/> objects. These are the LRS layers of the ELC map service.
		/// </summary>
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
		/// <param name="stream">A stream of JSON text representing route info</param>
		/// <returns></returns>
		private async static Task<Dictionary<string, List<RouteInfo>>> JsonToRouteInfoDict(Stream stream)
		{
			Dictionary<string, Dictionary<string, int>> intDict;

			var serializer = new JsonSerializer();
			using (var sReader = new StreamReader(stream))
			using (var jReader = new JsonTextReader(sReader))
			{
				intDict = await Task.Run(() => {
					return serializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(jReader);
				});
			}

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
		private async Task<RouteLocation[]> FindRouteLocationsAsync(IEnumerable<RouteLocation> locations, DateTime? referenceDate, object outSR, string lrsYear)
		{

			// Build the query string.
			var parameters = new Dictionary<string, string>();
			parameters.Add("f", "json");
			parameters.Add("locations", locations.ToJson());
			parameters.Add("referenceDate", referenceDate.HasValue ? referenceDate.Value.ToShortDateString() : string.Empty);
			if (outSR != null)
			{
				parameters.Add("outSR", outSR.ToString());
			}
			if (!string.IsNullOrEmpty(lrsYear))
			{
				parameters.Add("lrsYear", lrsYear);
			}
			
			byte[] parametersBytes = parameters.ToQueryStringBytes();

			var builder = new UriBuilder(this.Url + "/" + this.FindRouteLocationsOperationName);

			RouteLocation[] output = null;

			using (var client = new HttpClient())
			using (var httpContent = new FormUrlEncodedContent(parameters))
			{
				// TODO: Use Json.Net serializer instead of built-in .NET serializer. This will require custom converters.

				using (var httpResponseMessage = await client.PostAsync(builder.Uri, httpContent))
				using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync())
				////using (var streamReader = new StreamReader(stream))
				////using (var jsonReader = new JsonTextReader(streamReader))
				{
					////var jsonSerializer = JsonSerializer.CreateDefault();
					////output = jsonSerializer.Deserialize<RouteLocation[]>(jsonReader);

					output = await Task.Run(() => stream.ToRouteLocations<RouteLocation[]>());
				}
			}

			return output;
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
		public async Task<RouteLocation[]> FindRouteLocationsAsync(IEnumerable<RouteLocation> locations, DateTime? referenceDate, int? outSR, string lrsYear)
		{
			return await FindRouteLocationsAsync(locations, referenceDate, outSR as object, lrsYear);
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
		public async Task<RouteLocation[]> FindRouteLocationsAsync(IEnumerable<RouteLocation> locations, DateTime? referenceDate, string outSR, string lrsYear)
		{
			return await FindRouteLocationsAsync(locations, referenceDate, outSR as object, lrsYear);
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
		[Obsolete("Use FindRouteLocationsAsync instead.")]
		public RouteLocation[] FindRouteLocations(IEnumerable<RouteLocation> locations, DateTime? referenceDate, int? outSR, string lrsYear)
		{
			RouteLocation[] output = null;
			FindRouteLocationsAsync(locations, referenceDate, outSR as object, lrsYear).ContinueWith(f => {
				output = f.Result;
			}).Wait();
			return output;
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
		[Obsolete("Use FindRouteLocationsAsync instead.")]
		public RouteLocation[] FindRouteLocations(IEnumerable<RouteLocation> locations, DateTime? referenceDate, string outSR, string lrsYear)
		{
			RouteLocation[] output = null;
			FindRouteLocationsAsync(locations, referenceDate, outSR as object, lrsYear).ContinueWith(f => {
				output = f.Result;
			}).Wait();
			return output;
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
		public async Task<RouteLocation[]> FindNearestRouteLocationsAsync(IEnumerable<double> coordinates, DateTime referenceDate, double searchRadius,
			object inSR, object outSR, string lrsYear, string routeFilter = null)
		{

			// Build the query string.
			var parameters = new Dictionary<string, string>();
			parameters.Add("f", "json");
			parameters.Add("coordinates", JsonConvert.SerializeObject(coordinates));
			parameters.Add("referenceDate", referenceDate.ToShortDateString());
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

			RouteLocation[] routeLocations = null;

			using (var httpClient = new HttpClient())
			using (var httpContent = new FormUrlEncodedContent(parameters))
			using (var httpResponse = await httpClient.PostAsync(builder.Uri, httpContent))
			using (var stream = await httpResponse.Content.ReadAsStreamAsync())
			{
				
				routeLocations = await Task.Run(() => stream.ToRouteLocations<RouteLocation[]>());
			}

			return routeLocations;
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
		[Obsolete("Use FindNearestRouteLocationsAsync instead.")]
		public RouteLocation[] FindNearestRouteLocations(IEnumerable<double> coordinates, DateTime referenceDate, double searchRadius, 
			object inSR, object outSR, string lrsYear, string routeFilter = null)
		{
			RouteLocation[] routeLocations = null;
			FindNearestRouteLocationsAsync(coordinates, referenceDate, searchRadius, inSR, outSR, lrsYear, routeFilter).ContinueWith(t => {
				routeLocations = t.Result;
			}).Wait();
			return routeLocations;
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
		[Obsolete("Use FindNearestRouteLocationsAsync instead.")]
		public RouteLocation[] FindNearestRouteLocations<T>(IEnumerable<T> coordinates, DateTime referenceDate, double searchRadius,
			object inSR, object outSR, string lrsYear, string routeFilter = null) where T : class, IEnumerable<double>
		{
			return FindNearestRouteLocations(coordinates.SelectMany(c => c.Take(2)), referenceDate, searchRadius, inSR, outSR, lrsYear, routeFilter);
		}

		/// <summary>
		/// Finds a route that matches the info in <paramref name="routeInfo"/>.
		/// </summary>
		/// <param name="routeInfo">Provides information about the route.</param>
		/// <param name="lrsYear">Determines which year's LRS is used. Defaults to "Current" if omitted.</param>
		/// <param name="outSR">The output spatial reference system WKID. If omitted the output spatial reference will be the map service's default spatial reference.</param>
		/// <returns></returns>
		public async Task<Dictionary<LrsTypes, PolylineContract>> FindRouteAsync(RouteInfo routeInfo, string lrsYear = "Current", int? outSR = null)
		{
			var layers = this.Layers;
			LayerInfo parentLayer = string.IsNullOrEmpty(lrsYear) ? layers.First(l => l.ParentLayerId == -1) : layers.First(l => l.Name == lrsYear);
			IEnumerable<LayerInfo> subLayers = layers.Where(l => l.ParentLayerId == parentLayer.Id);
			Dictionary<LrsTypes, PolylineContract> output = new Dictionary<LrsTypes, PolylineContract>(2);

			/*
			 * http://wsdot.wa.gov/geosvcs/ArcGIS/rest/services/Shared/ElcRestSoe/MapServer/1/query?text=&where=RouteID+%3D+%27005%27&returnGeometry=true&maxAllowableOffset=&outSR=&f=json
			 */

			// Create the format string for the web requests.
			string fmt = string.Format("{0}/{{0}}/query?where=RouteID+%3D+%27{{1}}%27&returnGeometry=true&outSR={1}&f=json", 
				this.MapServerUrl, outSR.HasValue ? outSR.Value.ToString() : string.Empty);

			var jsSettings = new JsonSerializerSettings
			{
				MissingMemberHandling = MissingMemberHandling.Ignore
			};

			using (var client = new HttpClient())
			{
				if ((routeInfo.LrsTypes & LrsTypes.Increase) == LrsTypes.Increase)
				{
					var layerInfo = subLayers.FirstOrDefault(l => string.Compare(l.Name, "Increase", true) == 0);
					if (layerInfo != null)
					{
						string url = string.Format(fmt, layerInfo.Id, routeInfo.Name);
						PolylineContract feature = await QueryForFeature(jsSettings, url);
						if (feature != null)
						{
							output.Add(LrsTypes.Increase, feature);
						}
					}
				}
				if ((routeInfo.LrsTypes & LrsTypes.Decrease) == LrsTypes.Decrease)
				{
					var layerInfo = subLayers.FirstOrDefault(l => string.Compare(l.Name, "Decrease", true) == 0);
					if (layerInfo != null)
					{
						string url = string.Format(fmt, layerInfo.Id, routeInfo.Name);
						PolylineContract feature = await QueryForFeature(jsSettings, url);
						if (feature != null)
						{
							output.Add(LrsTypes.Decrease, feature);
						}
					}
				}
				if ((routeInfo.LrsTypes & LrsTypes.Ramp) == LrsTypes.Ramp)
				{
					var layerInfo = subLayers.FirstOrDefault(l => string.Compare(l.Name, "Ramp", true) == 0);
					if (layerInfo != null)
					{
						string url = string.Format(fmt, layerInfo.Id, routeInfo.Name);
						PolylineContract feature = await QueryForFeature(jsSettings, url);
						if (feature != null)
						{
							output.Add(LrsTypes.Ramp, feature);
						}
					}
				}
			}

			if (output.Count == 0)
			{
				output = null;
			}

			return output;
		}

		private static async Task<PolylineContract> QueryForFeature(JsonSerializerSettings jsSettings, string url)
		{
			PolylineContract output = null;
			RouteLayerQueryResponse response;
			using (var client = new HttpClient())
			using (var stream = await client.GetStreamAsync(url))
			using (var streamReader = new StreamReader(stream))
			using (var jsonReader = new JsonTextReader(streamReader))
			{
				var serializer = JsonSerializer.Create(jsSettings);
				response = await Task.Run(() => serializer.Deserialize<RouteLayerQueryResponse>(jsonReader));
			}
			var feature = response.features.FirstOrDefault();
			if (feature != null)
			{
				output = feature.geometry as PolylineContract;
			}
			return output;
		}

		/// <summary>
		/// Finds a route that matches the info in <paramref name="routeInfo"/>.
		/// </summary>
		/// <param name="routeInfo">Provides information about the route.</param>
		/// <param name="lrsYear">Determines which year's LRS is used. Defaults to "Current" if omitted.</param>
		/// <param name="outSR">The output spatial reference system WKID. If omitted the output spatial reference will be the map service's default spatial reference.</param>
		/// <returns></returns>
		[Obsolete("Use FindRouteAsync instead.")]
		public Dictionary<LrsTypes, PolylineContract> FindRoute(RouteInfo routeInfo, string lrsYear="Current", int? outSR=null)
		{
			Dictionary<LrsTypes, PolylineContract> output = null;
			FindRouteAsync(routeInfo, lrsYear, outSR).ContinueWith(t => {
				output = t.Result;
			}).Wait();

			return output;
		}
	}
}
