using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wsdot.Elc.Contracts;
using Wsdot.Elc.Wrapper;

namespace WrapperTest
{
#pragma warning disable 612, 618
	[TestClass]
	public class ElcAsyncTest
	{
		const string _defaultUrl = "http://www.wsdot.wa.gov/geoservices/arcgis/rest/services/Shared/ElcRestSOE/MapServer/exts/ElcRestSoe";
		const string _defaultFindRouteLocationsOperationName = "Find Route Locations";
		const string _defaultFindNearestRouteLocationsOperationName = "Find Nearest Route Locations";

		const string _urlPropName = "Url";
		const string _findRoutePropName = "Find Route Location Operation Name";
		const string _findNearestRoutePropName = "Find Nearest Route Location Operation Name";

		#region Get Routes Test
		private Dictionary<string, List<RouteInfo>> _routes;

		bool _getRoutesTestComplete = false;

		[TestMethod]
		public void TestGetRoutes()
		{
			ElcWrapper wrapper = ElcWrapper.GetInstance();
			wrapper.GetRoutesAsynch(OnGetRoutesComplete, OnGetRoutesException);

			while (!_getRoutesTestComplete)
			{
				System.Threading.Thread.Sleep(new TimeSpan(0, 0, 1));
			}

			Assert.IsNotNull(_routes);
		}

		private void OnGetRoutesComplete(Dictionary<string, List<RouteInfo>> dict)
		{
			_getRoutesTestComplete = true;
			_routes = dict;
		}

		private void OnGetRoutesException(Exception ex)
		{
			_getRoutesTestComplete = true;
			_routes = null;
		} 
		#endregion

		#region Find Route Locations test
		RouteLocation[] _routeLocationsFromFindRouteLocations;
		bool _findRouteLocatonsCompleted;

		[TestMethod]
		[TestProperty(_urlPropName, _defaultUrl)]
		[TestProperty(_findRoutePropName, _defaultFindRouteLocationsOperationName)]
		[TestProperty(_findNearestRoutePropName, _defaultFindNearestRouteLocationsOperationName)]
		[TestProperty("Route Locations", "[{\"Route\":\"005\", \"Arm\": 0, \"EndArm\": 100}]")]
		[TestProperty("Reference Date", "12/31/2011")]
		[TestProperty("Spatial Reference", "PROJCS[\"WGS_1984_Web_Mercator_Auxiliary_Sphere\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Mercator_Auxiliary_Sphere\"],PARAMETER[\"False_Easting\",0.0],PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",0.0],PARAMETER[\"Standard_Parallel_1\",0.0],PARAMETER[\"Auxiliary_Sphere_Type\",0.0],UNIT[\"Meter\",1.0]]")]
		[TestProperty("LRS Year", "Current")]
		public void TestFindRouteLocations()
		{
			// Get the test properties.
			var testProperties = (from attrib in MethodInfo.GetCurrentMethod().GetCustomAttributes(typeof(TestPropertyAttribute), false)
								  select (TestPropertyAttribute)attrib).ToDictionary(
									kvp => kvp.Name, kvp => kvp.Value
								 );

			ElcWrapper wrapper = ElcWrapper.GetInstance(
				testProperties[_urlPropName],
				testProperties[_findRoutePropName],
				testProperties[_findNearestRoutePropName]
				);

			// Get the input route locations from the test settings.
			RouteLocation[] locations = testProperties["Route Locations"].ToRouteLocations<RouteLocation[]>();

			// Get the reference date from the test properties.
			DateTime referenceDateTemp;
			DateTime.TryParse(testProperties["Reference Date"], out referenceDateTemp);
			DateTime? referenceDate =
				DateTime.TryParse(testProperties["Reference Date"], out referenceDateTemp) ? referenceDateTemp
				: default(DateTime?);

			// Get the LRS Year from the test properties.  If an empty string or whitespace, assume null.
			string lrsYear = testProperties["LRS Year"];
			if (string.IsNullOrWhiteSpace(lrsYear))
			{
				lrsYear = null;
			}

			int wkid;

			string srString = testProperties["Spatial Reference"];
			// If the provided SR is a WKID, convert to int.
			// Use the appropriate overload depending on if WKID or WKT was provided.
			if (int.TryParse(srString, out wkid))
			{
				wrapper.FindRouteLocationsAsync(locations, referenceDate, wkid, lrsYear,
					OnFindRouteLocations, OnFindRouteLocationsFail);
			}
			else
			{
				wrapper.FindRouteLocationsAsync(locations, referenceDate, string.IsNullOrWhiteSpace(srString) ? null : srString, lrsYear, OnFindRouteLocations, OnFindRouteLocationsFail);
			}

			while (!_findRouteLocatonsCompleted)
			{
				Thread.Sleep(new TimeSpan(0, 0, 1));
			}

			Assert.IsNotNull(_routeLocationsFromFindRouteLocations);
		}

		private void OnFindRouteLocations(RouteLocation[] routeLocations)
		{
			_routeLocationsFromFindRouteLocations = routeLocations;
			_findRouteLocatonsCompleted = true;
		}

		private void OnFindRouteLocationsFail(Exception error)
		{
			_findRouteLocatonsCompleted = true;
			_routeLocationsFromFindRouteLocations = null;
		} 
		#endregion

		bool _findNearestRouteLocatonsCompleted;
		RouteLocation[] _findNearestRouteLocationsResults = null;
		Exception _findNearestRouteLocationsException = null;

		[TestMethod]
		[TestProperty(_urlPropName, _defaultUrl)]
		[TestProperty(_findRoutePropName, _defaultFindRouteLocationsOperationName)]
		[TestProperty(_findNearestRoutePropName, _defaultFindNearestRouteLocationsOperationName)]
		public void TestFindNearestRouteLocation()
		{
			// Get the test properties.
			var testProperties = (from attrib in MethodInfo.GetCurrentMethod().GetCustomAttributes(typeof(TestPropertyAttribute), false)
								  select (TestPropertyAttribute)attrib).ToDictionary(
									kvp => kvp.Name, kvp => kvp.Value
								 );

			ElcWrapper wrapper = ElcWrapper.GetInstance(
				testProperties[_urlPropName],
				testProperties[_findRoutePropName],
				testProperties[_findNearestRoutePropName]
				);

			var coordinates = new double[][] { new double[] { -13685032.630180165, 5935861.0454789074 } };

			wrapper.FindNearestRouteLocationsAsync(coordinates, DateTime.Now, 200, 102100, 102100, "Current",
				OnFindNearestRouteLocationComplete, OnFindNearestRouteLocationFail, "LIKE '005%'");

			while (!_findNearestRouteLocatonsCompleted)
			{
				Thread.Sleep(new TimeSpan(0, 0, 1));
			}
			////var output = wrapper.FindNearestRouteLocations(coordinates, DateTime.Now, 200, 102100, 102100, "Current", "LIKE '005%'");

			Assert.IsNotNull(_findNearestRouteLocationsResults, "The results object was null.");
			Assert.IsTrue(_findNearestRouteLocationsResults.Length == coordinates.Length, "Length of input and output do not match.");
		}

		private void OnFindNearestRouteLocationComplete(RouteLocation[] locations)
		{
			_findNearestRouteLocationsResults = locations;
			_findNearestRouteLocatonsCompleted = true;
		}

		private void OnFindNearestRouteLocationFail(Exception ex)
		{
			_findNearestRouteLocationsException = ex;
			_findNearestRouteLocatonsCompleted = true;
		}
	}
#pragma warning restore 612, 618
}
