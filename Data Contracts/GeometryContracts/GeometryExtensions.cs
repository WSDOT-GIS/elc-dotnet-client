namespace Wsdot.Geometry.Contracts
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web.Script.Serialization;
    using ESRI.ArcGIS.SOAP;
    using System.Configuration;
    using System.Linq;
    using System.Collections.Generic;
    using System.Web.Services.Protocols;


    /// <summary>
    /// Extensions to geometry objects.
    /// </summary>
    public static class GeometryExtensions
    {


        // Create regexes that match different types of double arrays representing geometry.

        // Matches a double[][][] (polygon or polyline)
        static readonly Regex _polyRegex = new Regex(@"(?xn)
\[\s*
    #Paths
    \[\s*
        #Points
        (
        \[\s*
            (-?[\d.]+,?\s*){2,3}
        \s*\]\s*,?\s*
        )+
    \]\s*
\]
");
        // Matches a double[][] (path or ring)
        static readonly Regex _pathRegex = new Regex(@"(?xn)
    \[\s*
        #Points
        (
        \[\s*
            (-?[\d.]+,?\s*){2,3}
        \s*\]\s*,?\s*
        )+
    \]\s*
");
        // Matches a double[] (point)
        static readonly Regex _pointRegex = new Regex(@"(?xn)
(\[\s*
        (-?[\d.]+,?\s*){2,3}
    \s*\]\s*,?\s*
)+");

        /// <summary>
        /// Uses a <see cref="Regex"/> to determine what type of array 
        /// (double[], double[][], or double[][][]) 
        /// a JSON string represents, deserializes the string to an array, then converts 
        /// the array into a <see cref="Geometry"/> object.
        /// </summary>
        /// <param name="searchShapeString">A JSON string representing an array of doubles.</param>
        /// <returns>A <see cref="Geometry"/></returns>
        /// <exception cref="ArgumentException">
        /// <see langword="throw">Thrown</see> if <paramref name="searchShapeString"/> cannot be deserialized into an array representing geometry.
        /// </exception>
        public static Geometry ToGeometry(this string searchShapeString, string wktOrWkid=null)
        {
            Geometry geometry = null;
            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            SpatialReference sr = null;
            if (wktOrWkid != null)
            {
                sr = wktOrWkid.ToSpatialReference();
            }

            if (_polyRegex.IsMatch(searchShapeString))
            {
                double[][][] array = _serializer.Deserialize<double[][][]>(searchShapeString);
                geometry = array.ToGeometry(sr);
            }
            else if (_pathRegex.IsMatch(searchShapeString))
            {
                double[][] array = _serializer.Deserialize<double[][]>(searchShapeString);
                geometry = array.ToGeometry(sr);
            }
            else if (_pointRegex.IsMatch(searchShapeString))
            {
                double[] array = _serializer.Deserialize<double[]>(searchShapeString);
                geometry = array.ToGeometry(sr);
            }
            else
            {
                throw new ArgumentException("Input string must be a JSON array of doubles of 1, 2, or 3 nesting levels.", searchShapeString);
            }

            return geometry;
        }

        ////public static GeometryContract ToGeomertyContract(this string geometryString, string wktOrWkid=null) {
        ////    GeometryContract geometry = null;
        ////    JavaScriptSerializer _serializer = new JavaScriptSerializer();

        ////    SpatialReferenceContract sr = null;
        ////    if (wktOrWkid != null)
        ////    {
        ////        sr = wktOrWkid.ToSpatialReference();
        ////    }

        ////    if (_polyRegex.IsMatch(geometryString))
        ////    {
        ////        double[][][] array = _serializer.Deserialize<double[][][]>(geometryString);
        ////        geometry = array.ToGeometryContract(sr);
        ////    }
        ////    else if (_pathRegex.IsMatch(geometryString))
        ////    {
        ////        double[][] array = _serializer.Deserialize<double[][]>(geometryString);
        ////        geometry = array.ToGeometry(sr);
        ////    }
        ////    else if (_pointRegex.IsMatch(geometryString))
        ////    {
        ////        double[] array = _serializer.Deserialize<double[]>(geometryString);
        ////        geometry = array.ToGeometry(sr);
        ////    }
        ////    else
        ////    {
        ////        throw new ArgumentException("Input string must be a JSON array of doubles of 1, 2, or 3 nesting levels.", searchShapeString);
        ////    }

        ////    return geometry;
        ////}

        public static GeometryContract ToGeometryContract(this string arrayRepresentation, JavaScriptSerializer jsSerializer = null)
        {
            if (jsSerializer == null)
            {
                jsSerializer = new JavaScriptSerializer();
            }
            object array = jsSerializer.DeserializeObject(arrayRepresentation);
            Type t = array.GetType();
            if (t.IsAssignableFrom(typeof(double[])))
            {
                return ((double[])array).ToGeometryContract();
            }
            else if (t.IsAssignableFrom(typeof(double[][])))
            {
                return ((double[][])array).ToGeometryContract();
            }
            else if (t.IsAssignableFrom(typeof(double[][][])))
            {
                return ((double[][][])array).ToGeometryContract();
            }
            else
            {
                throw new FormatException("The format of the arrayRepresentation parameter is not correct.");
            }
        }

        public static PointContract ToGeometryContract(this double[] coordinates, SpatialReferenceContract sr = null)
        {
            return new PointContract { X = coordinates[0], Y = coordinates[1], SpatialReference = sr};
        }

        public static GeometryContract ToGeometryContract(this double[][][] coordinates, SpatialReferenceContract sr = null)
        {
            // Get the number of paths that are rings (i.e., the first and last point in the path are the same).
            var ringCount = coordinates.Count(FirstAndLastPointAreEqual);

            if (ringCount == coordinates.Length)
            {
                return new PolygonContract { Rings = coordinates, SpatialReference = sr };
            }
            else
            {
                return new PolylineContract { Paths = coordinates, SpatialReference = sr };
            }
        }

        public static GeometryContract ToGeometryContract(this double[][] coordinates, SpatialReferenceContract sr = null)
        {
            double[][][] input = new double[1][][];
            input[0] = coordinates;
            return ToGeometryContract(input, sr);
        }

        /// <summary>
        /// Determines if the first and last point defined in <paramref name="path"/> are the same point.
        /// </summary>
        /// <param name="path">A jagged array of doubles representing an array of points.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the first and last points are the same, 
        /// <see langword="false"/> if they are not the same or if there are fewer than two points in the array.
        /// </returns>
        /// <exception cref="ArgumentNullException">Returned if <paramref name="path"/> is <see langword="null"/>.</exception>
        public static bool FirstAndLastPointAreEqual(this double[][] path)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (path.Length < 2) return false;
            double[] first = path[0];
            double[] last = path[path.Length - 1];
            const int X = 0, Y = 1;
            return first[X] == last[X] && first[Y] == last[Y];
        }

        /////// <summary>
        /////// Creates a <see cref="SpatialReference"/> using either a WKID or WKT contained in a string.
        /////// </summary>
        /////// <param name="wktOrWkid">A string containing either a WKT or WKID.</param>
        /////// <returns>Returns a <see cref="SpatialReference"/> object.</returns>
        ////public static SpatialReference ToSpatialReference(this string wktOrWkid)
        ////{
        ////    var settings = ConfigurationManager.AppSettings;
        ////    int wkid;
        ////    if (int.TryParse(wktOrWkid, out wkid))
        ////    {
        ////        return wkid.ToSpatialReference();
        ////    }
        ////    else
        ////    {
        ////        SpatialReference output;
        ////        using (var geometryServer = new GeometryServerProxy { Url = settings["GeometryServer"] })
        ////        {
        ////            try
        ////            {
        ////                output = geometryServer.FindSRByWKT(wktOrWkid, null, true, true);
        ////            }
        ////            catch (SoapException ex)
        ////            {
        ////                throw new ArgumentException("The spatial reference string must contain either the WKID or WKT of a spatial reference.", ex);
        ////            }
        ////        }
        ////        return output;
        ////    }
        ////}

        /////// <summary>
        /////// Creates a <see cref="SpatialReference"/> object using the given <see cref="WKID"/> integer.
        /////// </summary>
        /////// <param name="wkid">A Well-Known Identifier (WKID) of a spatial reference.</param>
        /////// <returns>Returns a <see cref="SpatialReference"/> object.</returns>
        ////public static SpatialReference ToSpatialReference(this int wkid)
        ////{
        ////    var settings = ConfigurationManager.AppSettings;
        ////    char[] delimeters = new char[] { ',' };
        ////    var splitOptions = StringSplitOptions.RemoveEmptyEntries;
        ////    IEnumerable<int> wkids = from str in settings["ProjectedWkids"].Split(delimeters, splitOptions) select int.Parse(str);
        ////    if (wkids.Contains(wkid))
        ////    {
        ////        return new ProjectedCoordinateSystem { WKID = wkid, WKIDSpecified = true };
        ////    }

        ////    wkids = from str in settings["GeographicWkids"].Split(delimeters, splitOptions) select int.Parse(str);
        ////    if (wkids.Contains(wkid))
        ////    {
        ////        return new GeographicCoordinateSystem { WKID = wkid, WKIDSpecified = true };
        ////    }

        ////    SpatialReference output;
        ////    using (var geometryServer = new GeometryServerProxy { Url = settings["GeometryServer"] })
        ////    {
        ////        output = geometryServer.FindSRByWKID(null, wkid, -1, true, true);
        ////    }
        ////    return output;

        ////}
    }

}