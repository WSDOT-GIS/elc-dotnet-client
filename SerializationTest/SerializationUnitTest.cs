using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SerializationTest.Properties;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wsdot.Elc.Contracts;
using Wsdot.Elc.Serialization;
using Wsdot.Geometry.Contracts;

namespace SerializationTest
{
    [TestClass]
    public class SerializationUnitTest
    {
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Reads a sample list of routes from a CSV table.
        /// </summary>
        /// <returns>A <see cref="Dictionary&lt;K,V&gt;"/> of <see cref="RouteInfo"/> objects keyed by route names.</returns>
        private Dictionary<string, RouteInfo> GetRouteInfosFromTable()
        {
            const string CSV_PATH = "Routes.csv";

            if (!File.Exists(CSV_PATH))
            {
                throw new FileNotFoundException("File not found", Path.GetFullPath(CSV_PATH));
            }

            var output = new Dictionary<string, RouteInfo>();


            using (var textReader = new StreamReader(CSV_PATH))
            using (var csvReader = new CsvReader(textReader))
            {
                while (csvReader.Read())
                {
                    var routeId = csvReader.GetField<string>("RouteID");
                    var routeDirection = csvReader.GetField<char?>("Direction");
                    var routeType = csvReader.GetField<RouteType>("RT_TYPEB");
                    LrsTypes lrsTypes = LrsTypes.None;

                    if (routeDirection.HasValue)
                    {
                        switch (routeDirection.Value)
                        {
                            case 'i':
                                lrsTypes = LrsTypes.Increase;
                                break;
                            case 'd':
                                lrsTypes = LrsTypes.Decrease;
                                break;
                            case 'b':
                                lrsTypes = LrsTypes.Both;
                                break;
                        }
                    }

                    switch (routeType)
                    {
                        case RouteType.RA:
                            lrsTypes = LrsTypes.Ramp;
                            break;
                    }

                    if (output.Keys.Contains(routeId))
                    {
                        output[routeId].LrsTypes |= lrsTypes;
                    }
                    else
                    {
                        output.Add(routeId, new RouteInfo
                        {
                            Name = routeId,
                            LrsTypes = lrsTypes,
                            RouteType = routeType
                        });
                    }
                }
            }

            return output;
        }

        [TestMethod]
        public void SerializeRouteInfos()
        {
            var routeInfos = GetRouteInfosFromTable();
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Converters.Add(new RouteListConverter());
            string path = "Routes.json";
            path = Path.Combine(TestContext.ResultsDirectory, path);

            using (var textWriter = new StreamWriter(path))
            {
                serializer.Serialize(textWriter, routeInfos);
            }
            if (File.Exists(path))
            {
                TestContext.AddResultFile(path);
            }
        }

        [TestMethod]
        public void DeserializePoint()
        {
            const double
                x = -122.345,
                y = 48.72345;
            const int wkid = 4326;
            string json = string.Format("{{\"x\":{0}, \"y\":{1}, \"spatialReference\": {{\"wkid\":{2}}} }}", x, y, wkid);
            var point = JsonConvert.DeserializeObject<PointContract>(json);
            const string shouldBeFmt = "{0} should be {1}";
            Assert.AreEqual(x, point.X, shouldBeFmt, "X", x);
            Assert.AreEqual(y, point.Y, shouldBeFmt, "Y", y);
            Assert.AreEqual(wkid, point.SpatialReference.Wkid, shouldBeFmt, "SpatialReference.Wkid", wkid);

            var converter = new GeometryConverter();

            var geom = JsonConvert.DeserializeObject<GeometryContract>(json, converter);
            Assert.IsInstanceOfType(geom, typeof(PointContract));
        }

        [TestMethod]
        public void DeserializeRouteLocationsArray()
        {
            string json = Resources.RouteLocations;
            var converter = new GeometryConverter();

            var locations = JsonConvert.DeserializeObject<RouteLocation[]>(json, converter);
            CollectionAssert.AllItemsAreNotNull(locations, "Items in array should not be null.");
            Assert.AreEqual(1, locations.Length, "Array should have a single element.");
            var location = locations.First();
            Assert.AreEqual(155.49741407457302, location.Angle);
            Assert.AreEqual(0, location.Arm);
            Assert.AreEqual(0, location.ArmCalcReturnCode);
            Assert.IsFalse(location.Back.Value);
            Assert.IsFalse(location.Decrease.Value);
            Assert.IsInstanceOfType(location.EventPoint, typeof(PointContract));
        }
    }
}
