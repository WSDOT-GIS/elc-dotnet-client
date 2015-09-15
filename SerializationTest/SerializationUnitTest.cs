using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SerializationTest.Properties;
using System.Linq;
using Wsdot.Elc.Contracts;
using Wsdot.Elc.Serialization;
using Wsdot.Geometry.Contracts;

namespace SerializationTest
{
	[TestClass]
	public class SerializationUnitTest
	{
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
