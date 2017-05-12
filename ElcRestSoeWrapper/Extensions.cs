using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Wsdot.Elc.Contracts;

namespace Wsdot.Elc.Wrapper
{
    /// <summary>
    /// Provides extension methods to exisiting classes.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts an enumeration of <see cref="RouteLocation"/> objects into a JSON string.
        /// </summary>
        /// <param name="locations">An enumeration of <see cref="RouteLocation"/>s</param>
        /// <returns>The JSON equivalent of <paramref name="locations"/> </returns>
        public static string ToJson(this IEnumerable<RouteLocation> locations)
        {
            if (locations == null)
            {
                return null;
            }

            var serializer = new DataContractJsonSerializer(typeof(RouteLocation[]));
            byte[] bytes;
            string json;

            using (var memStream = new MemoryStream())
            {
                serializer.WriteObject(memStream, locations.ToArray());
                bytes = memStream.ToArray();

            }

            json = Encoding.UTF8.GetString(bytes);
            bytes = null;

            return json;
        }

        /// <summary>
        /// Deserializes a JSON string into a set of <see cref="RouteLocation"/>s
        /// </summary>
        /// <typeparam name="T">A type that implements an <see cref="IEnumerable&lt;T&gt;"/> of <see cref="RouteLocation"/>.</typeparam>
        /// <param name="json">A JSON representation of an array of <see cref="RouteLocation"/> objects.</param>
        /// <returns>An <see cref="IEnumerable&lt;T&gt;"/> of <see cref="RouteLocation"/></returns>
        public static T ToRouteLocations<T>(this string json) where T : class, IEnumerable<RouteLocation>
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            byte[] bytes = Encoding.UTF8.GetBytes(json);

            T locations;

            using (var stream = new MemoryStream(bytes))
            {
                locations = serializer.ReadObject(stream) as T;
            }

            return locations;
        }

        /// <summary>
        /// Deserializes a JSON string into a set of <see cref="RouteLocation"/>s
        /// </summary>
        /// <typeparam name="T">A type that implements an <see cref="IEnumerable&lt;T&gt;"/> of <see cref="RouteLocation"/>.</typeparam>
        /// <param name="json">A JSON representation of an array of <see cref="RouteLocation"/> objects.</param>
        /// <returns>An <see cref="IEnumerable&lt;T&gt;"/> of <see cref="RouteLocation"/></returns>
        public static T ToRouteLocations<T>(this Stream json) where T : class, IEnumerable<RouteLocation>
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            T locations = serializer.ReadObject(json) as T;

            return locations;
        }

        /// <summary>
        /// Converts a dictionary into a query string.
        /// </summary>
        /// <param name="parameters">A dictionary with string keys and string values.</param>
        /// <returns>Returns a query string.</returns>
        public static string ToQueryString(this IDictionary<string, string> parameters)
        {
            var count = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in parameters)
            {
                if (count > 0)
                {
                    sb.AppendFormat("&{0}={1}", kvp.Key, kvp.Value);
                }
                else
                {
                    sb.AppendFormat("{0}={1}", kvp.Key, kvp.Value);
                }
                count++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Runs the <see cref="Extensions.ToQueryString(IDictionary&lt;string, string&gt;)"/> method and then converts its
        /// output into a <see cref="byte"/> array.
        /// </summary>
        /// <param name="parameters">A dictionary of key and value strings.</param>
        /// <returns>A <see cref="byte"/> array.</returns>
        public static byte[] ToQueryStringBytes(this IDictionary<string, string> parameters)
        {
            string qs = parameters.ToQueryString();
            return Encoding.ASCII.GetBytes(qs);
        }


        public static string ToShortDateString(this DateTime date)
        {
            return date.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
        }
    }
}
