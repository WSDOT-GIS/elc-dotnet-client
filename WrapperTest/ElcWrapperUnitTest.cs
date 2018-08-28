using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        const string _wgs84Wkt = "PROJCS[\"WGS_1984_Web_Mercator_Auxiliary_Sphere\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Mercator_Auxiliary_Sphere\"],PARAMETER[\"False_Easting\",0.0],PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",0.0],PARAMETER[\"Standard_Parallel_1\",0.0],PARAMETER[\"Auxiliary_Sphere_Type\",0.0],UNIT[\"Meter\",1.0]]";

        private ElcWrapper _elcClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _elcClient = new ElcWrapper(_defaultUrl, _defaultFindRouteLocationsOperationName, _defaultFindNearestRouteLocationsOperationName);
        }

        private TestContext _testContext;

        public TestContext TestContext
        {
            get { return _testContext; }
            set { _testContext = value; }
        }

        private static bool HasArmCalcErrors(IEnumerable<RouteLocation> locations)
        {
            return (from l in locations
                    where l.ArmCalcReturnCode != 0 && l.ArmCalcEndReturnCode != 0
                    select l).Count() > 0;
        }


        [TestMethod]
        public async Task TestFindRouteLocations()
        {
            // Get the input route locations from the test settings.
            RouteLocation[] locations = "[{\"Route\":\"005\", \"Arm\": 0, \"EndArm\": 100}]".ToRouteLocations<RouteLocation[]>();

            // Get the reference date from the test properties.
            DateTime referenceDate = new DateTime(2011, 12, 31);

            // Get the LRS Year from the test properties.  If an empty string or whitespace, assume null.
            const string lrsYear = "Current";


            RouteLocation[] routeLocations;

            string srString = _wgs84Wkt;

            // If the provided SR is a WKID, convert to int.
            // Use the appropriate overload depending on if WKID or WKT was provided.
            if (int.TryParse(srString, out int wkid))
            {

                routeLocations = await _elcClient.FindRouteLocations(locations, referenceDate, wkid, lrsYear);

            }
            else
            {

                routeLocations = await _elcClient.FindRouteLocations(locations, referenceDate, string.IsNullOrWhiteSpace(srString) ? null : srString, lrsYear);

            }

            Assert.IsTrue(routeLocations.Length == locations.Length, "Length of input and output collections do not match.");
        }

        [TestMethod]
        public async Task TestFindRouteLocationsWithInlineReferenceDates()
        {
            const string routeLocationsJson = "[{\"Route\":\"005\", \"Arm\": 0, \"EndArm\": 100, \"ReferenceDate\": \"12/31/2011\"}]";

            // Get the input route locations from the test settings.
            var locations = routeLocationsJson.ToRouteLocations<RouteLocation[]>();

            // Get the reference date from the test properties.
            DateTime? referenceDate = default(DateTime?);

            // Get the LRS Year from the test properties.  If an empty string or whitespace, assume null.
            string lrsYear = "Current";

            RouteLocation[] routeLocations;

            const string srString = _wgs84Wkt;

            // If the provided SR is a WKID, convert to int.
            // Use the appropriate overload depending on if WKID or WKT was provided.
            if (int.TryParse(srString, out int wkid))
            {

                routeLocations = await _elcClient.FindRouteLocations(locations, referenceDate, wkid, lrsYear);
            }
            else
            {
                routeLocations = await _elcClient.FindRouteLocations(locations, referenceDate, string.IsNullOrWhiteSpace(srString) ? null : srString, lrsYear);
            }


            Assert.IsTrue(routeLocations.Length == locations.Length, "Length of input and output collections do not match.");
            Assert.IsFalse(HasArmCalcErrors(routeLocations), "One or more ArmCalc errors occured.");
        }

        [TestMethod]
        public async Task TestFindNearestRouteLocation()
        {
            // Get the test properties.
            var coordinates = new double[] { -13685032.630180165, 5935861.0454789074 };
            var output = await _elcClient.FindNearestRouteLocations(coordinates, DateTime.Now, 200, 102100, 102100, "Current", "LIKE '005%'");

            Assert.AreEqual(1, output.Length, "Length of input and output do not match.");
        }

        [TestMethod]
        public void TestMisc()
        {
            var json = Extensions.ToJson(null);
            Assert.IsNull(json);
        }

        [TestMethod]
        public void TestRoutesList()
        {
            // Get the test properties.
            var routes = _elcClient.Routes;

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
            RouteInfo r101Both = new RouteInfo
            {
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

            var routeInfo = new List<RouteInfo>(new[]
            {
                r101Both,
                r005Both,
                r005Inc
            });

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
                bytes = memStream.ToArray();
                json = Encoding.UTF8.GetString(bytes);
            }
            Assert.IsFalse(string.IsNullOrWhiteSpace(json));


            // Serialize using both built-in and JSON.NET and compare results.
            using (MemoryStream memStream = new MemoryStream(bytes))
            {
                settings = (ElcSettings)serializer.ReadObject(memStream);
            }
            settings2 = JsonConvert.DeserializeObject<ElcSettings>(jsonDotNetJson);

            Assert.AreEqual<ElcSettings>(settings, settings2, "ElcSettings deserialized via different methods should be equal.");
        }

        [TestMethod]
        public async Task TestFindRoute()
        {

            var result = await _elcClient.FindRoute(new RouteInfo { Name = "005", LrsTypes = LrsTypes.Both });


            Assert.IsTrue(result.Keys.Count == 2, "There should be two results for 005: Increase and Decrease");
            Assert.IsNotNull(result[LrsTypes.Increase], "Increase geometry should not be null.");
            Assert.IsNotNull(result[LrsTypes.Decrease], "Decrease geometry should not be null.");
        }
    }
}
