using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Wsdot.Elc.Contracts;
using Wsdot.Elc.Wrapper;

namespace WrapperTest
{
	[TestClass]
	public class ElcWrapperUnitTest
	{
		const string _defaultUrl = "http://www.wsdot.wa.gov/geoservices/arcgis/rest/services/Shared/ElcRestSOE/MapServer/exts/ElcRestSoe";
		const string _defaultFindRouteLocationsOperationName = "Find Route Locations";
		const string _defaultFindNearestRouteLocationsOperationName = "Find Nearest Route Locations";

		const string _urlPropName = "Url";
		const string _findRoutePropName = "Find Route Location Operation Name";
		const string _findNearestRoutePropName = "Find Nearest Route Location Operation Name";

		private TestContext _testContext;

		public TestContext TestContext
		{
			get { return _testContext; }
			set { _testContext = value; }
		}

		private bool HasArmCalcErrors(IEnumerable<RouteLocation> locations)
		{
			return (from l in locations
				   where l.ArmCalcReturnCode != 0 && l.ArmCalcEndReturnCode != 0
				   select l).Count() > 0;
		}


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
			DateTime? referenceDate = DateTime.TryParse(testProperties["Reference Date"], out referenceDateTemp) ? referenceDateTemp : default(DateTime?);

			// Get the LRS Year from the test properties.  If an empty string or whitespace, assume null.
			string lrsYear = testProperties["LRS Year"];
			if (string.IsNullOrWhiteSpace(lrsYear))
			{
				lrsYear = null;
			}

			int wkid;

			RouteLocation[] routeLocations;

			string srString = testProperties["Spatial Reference"];

			// If the provided SR is a WKID, convert to int.
			// Use the appropriate overload depending on if WKID or WKT was provided.
			if (int.TryParse(srString, out wkid))
			{
				routeLocations = wrapper.FindRouteLocations(locations, referenceDate, wkid, lrsYear);
			}
			else
			{
				routeLocations = wrapper.FindRouteLocations(locations, referenceDate, string.IsNullOrWhiteSpace(srString) ? null : srString, lrsYear);
			}

			Assert.IsTrue(routeLocations.Length == locations.Length, "Length of input and output collections do not match.");
		}

		[TestMethod]
		[TestProperty(_urlPropName, _defaultUrl)]
		[TestProperty(_findRoutePropName, _defaultFindRouteLocationsOperationName)]
		[TestProperty(_findNearestRoutePropName, _defaultFindNearestRouteLocationsOperationName)]
		[TestProperty("Route Locations", "[{\"Route\":\"005\", \"Arm\": 0, \"EndArm\": 100, \"ReferenceDate\": \"12/31/2011\"}]")]
		[TestProperty("Reference Date", "")]
		[TestProperty("Spatial Reference", "PROJCS[\"WGS_1984_Web_Mercator_Auxiliary_Sphere\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Mercator_Auxiliary_Sphere\"],PARAMETER[\"False_Easting\",0.0],PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",0.0],PARAMETER[\"Standard_Parallel_1\",0.0],PARAMETER[\"Auxiliary_Sphere_Type\",0.0],UNIT[\"Meter\",1.0]]")]
		[TestProperty("LRS Year", "Current")]
		public void TestFindRouteLocationsWithInlineReferenceDates()
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
			DateTime? referenceDate = DateTime.TryParse(testProperties["Reference Date"], out referenceDateTemp) ? referenceDateTemp : default(DateTime?);

			// Get the LRS Year from the test properties.  If an empty string or whitespace, assume null.
			string lrsYear = testProperties["LRS Year"];
			if (string.IsNullOrWhiteSpace(lrsYear))
			{
				lrsYear = null;
			}

			int wkid;

			RouteLocation[] routeLocations;

			string srString = testProperties["Spatial Reference"];

			// If the provided SR is a WKID, convert to int.
			// Use the appropriate overload depending on if WKID or WKT was provided.
			if (int.TryParse(srString, out wkid))
			{
				routeLocations = wrapper.FindRouteLocations(locations, referenceDate, wkid, lrsYear);
			}
			else
			{
				routeLocations = wrapper.FindRouteLocations(locations, referenceDate, string.IsNullOrWhiteSpace(srString) ? null : srString, lrsYear);
			}

			Assert.IsTrue(routeLocations.Length == locations.Length, "Length of input and output collections do not match.");
			Assert.IsFalse(HasArmCalcErrors(routeLocations), "One or more ArmCalc errors occured.");
		}

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

			var coordinates = new double[][] { new double[] {-13685032.630180165, 5935861.0454789074} };
			var output = wrapper.FindNearestRouteLocations(coordinates, DateTime.Now, 200, 102100, 102100, "Current", "LIKE '005%'");

			Assert.IsTrue(output.Count() == coordinates.Length, "Length of input and output do not match.");
		}

		[TestMethod]
		public void TestMisc()
		{
			var json = Extensions.ToJson(null);
			Assert.IsNull(json);
		}

		[TestMethod]
		[TestProperty(_urlPropName, _defaultUrl)]
		public void TestRoutesList()
		{
			// Get the test properties.
			var testProperties = (from attrib in MethodInfo.GetCurrentMethod().GetCustomAttributes(typeof(TestPropertyAttribute), false)
								  select (TestPropertyAttribute)attrib).ToDictionary(
									kvp => kvp.Name, kvp => kvp.Value
								 );

			ElcWrapper wrapper = ElcWrapper.GetInstance(testProperties[_urlPropName]);

			var routes = wrapper.Routes;

			Assert.IsFalse(routes == null);
			Assert.IsTrue(routes.Count > 0, "There should be at least one element corresponding to an LRS year.");
			Assert.IsTrue(routes.ContainsKey("Current"), "There should be a \"Current\" key.");
			Assert.IsNotNull(routes["Current"].Select(ri => ri.Name == "005").FirstOrDefault());
		}

		/// <summary>
		/// Tests the equality and comparison methods on <see cref="RouteInfo"/>
		/// </summary>
		[TestMethod]
		public void RouteInfoEqualityTest()
		{
			RouteInfo r1, r2;
			// Test two values that should be equal.
			r1 = new RouteInfo
			{
				Name = "005",
				LrsTypes = LrsTypes.Both
			};

			r2 = new RouteInfo
			{
				Name = "005",
				LrsTypes = LrsTypes.Both
			};

			Assert.AreEqual<RouteInfo>(r1, r2, "{0} should equal {1}.", r1, r2);

			r2.LrsTypes = LrsTypes.Decrease;

			Assert.AreNotEqual<RouteInfo>(r1, r2, "{0} should not equal {1}", r1, r2);
		}

		/// <summary>
		/// Tests <see cref="RouteInfo"/>'s implementation of <see cref="IComparable"/> by sorting a list of <see cref="RouteInfo"/> objects.
		/// </summary>
		[TestMethod]
		public void RouteInfoComparisonTest()
		{
			// Create route infos and add them to a list out of order.
			RouteInfo r101Both = new RouteInfo { 
				Name = "101",
				LrsTypes = LrsTypes.Both
			};
			RouteInfo r005Both = new RouteInfo
			{
				Name = "005",
				LrsTypes = LrsTypes.Both
			};
			RouteInfo r005Inc = new RouteInfo
			{
				Name = "005",
				LrsTypes = LrsTypes.Increase
			};

			List<RouteInfo> routeInfo = new List<RouteInfo>();
			routeInfo = new List<RouteInfo>(3);
			routeInfo.Add(r101Both);
			routeInfo.Add(r005Both);
			routeInfo.Add(r005Inc);

			// Sort the list and test for the expected order.
			routeInfo.Sort();

			Assert.AreEqual(routeInfo[0], r005Inc);
			Assert.AreEqual(routeInfo[1], r005Both);
			Assert.AreEqual(routeInfo[2], r101Both);
		}

		[TestMethod]
		public void TestElcSettingsSerialization()
		{
			// Create ELC settings object with default parameter values.
			var settings = new ElcSettings();
			Assert.IsFalse(string.IsNullOrWhiteSpace(settings.Url));
			Assert.IsFalse(string.IsNullOrWhiteSpace(settings.FindRouteLocationOperationName));
			Assert.IsFalse(string.IsNullOrWhiteSpace(settings.FindNearestRouteLocationOperationName));
			Assert.IsFalse(string.IsNullOrWhiteSpace(settings.RoutesResourceName));

			// Note that for this test the URL will not actually be queried.
			const string testUrl = "http://hqolymgis99t/arcgis/rest/services/Shared/ElcRestSOE/MapServer/exts/ElcRestSoe";

			var settings2 = new ElcSettings(testUrl, null, null, null);

			Assert.AreNotEqual<ElcSettings>(settings, settings2, "Settings with differing URLs passed to constructors should not be equal.");

			// Deserialize to JSON using JSON.NET.
			var jsonDotNetJson = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
			Assert.IsFalse(string.IsNullOrWhiteSpace(jsonDotNetJson));

			// Deserialize using built-in .NET methods.
			var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(ElcSettings));
			string json;
			byte[] bytes;
			using (MemoryStream memStream = new MemoryStream())
			{
				serializer.WriteObject(memStream, settings);
				bytes = memStream.GetBuffer();
				json = Encoding.Default.GetString(bytes);
			}
			Assert.IsFalse(string.IsNullOrWhiteSpace(json));


			// Serialize using both built-in and JSON.NET and compare results.
			using (MemoryStream memStream = new MemoryStream(bytes))
			{
				settings = (ElcSettings)serializer.ReadObject(memStream);
			}
			settings2 = Newtonsoft.Json.JsonConvert.DeserializeObject<ElcSettings>(jsonDotNetJson);

			Assert.AreEqual<ElcSettings>(settings, settings2, "ElcSettings deserialized via different methods should be equal.");
		}

		[TestMethod]
		[TestProperty(_urlPropName, _defaultUrl)]
		[TestProperty(_findRoutePropName, _defaultFindRouteLocationsOperationName)]
		[TestProperty(_findNearestRoutePropName, _defaultFindNearestRouteLocationsOperationName)]
		public void TestFindRoute()
		{
			// Get the test properties.
			var testProperties = (from attrib in MethodInfo.GetCurrentMethod().GetCustomAttributes(typeof(TestPropertyAttribute), false)
								  select (TestPropertyAttribute)attrib).ToDictionary(
									kvp => kvp.Name, kvp => kvp.Value
								 );

			var wrapper = ElcWrapper.GetInstance(testProperties[_urlPropName], testProperties[_findRoutePropName], testProperties[_findNearestRoutePropName]);

			var result = wrapper.FindRoute(new RouteInfo { Name = "005", LrsTypes = LrsTypes.Both });

			Assert.IsTrue(result.Keys.Count == 2, "There should be two results for 005: Increase and Decrease");
			Assert.IsNotNull(result[LrsTypes.Increase], "Increase geometry should not be null.");
			Assert.IsNotNull(result[LrsTypes.Decrease], "Decrease geometry should not be null.");
		}
	}
}
