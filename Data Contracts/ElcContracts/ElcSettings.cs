﻿using System;
using System.Runtime.Serialization;

namespace Wsdot.Elc.Contracts
{
    /// <summary>
    /// Settings for the ELC REST SOE endpoint.
    /// </summary>
    [DataContract]
    public class ElcSettings : IEquatable<ElcSettings>
    {
        private const string
            _defaultUrl = "https://data.wsdot.wa.gov/arcgis/rest/services/Shared/ElcRestSOE/MapServer/exts/ElcRestSoe",
            _defaultFindRouteLocationOperationName = "Find Route Locations",
            _defaultFindNearestRouteLocationsOperationName = "Find Nearest Route Locations",
            _defaultRoutesResourceName = "routes";

        /// <summary>
        /// The default routes resource name.
        /// </summary>
        public static string DefaultRoutesResourceName
        {
            get { return _defaultRoutesResourceName; }
        }


        /// <summary>
        /// The default name of the Find Nearest Route Locations operation.
        /// </summary>
        public static string DefaultFindNearestRouteLocationsOperationName
        {
            get { return _defaultFindNearestRouteLocationsOperationName; }
        }


        /// <summary>
        /// The default name of the Find Route Location operation name.
        /// </summary>
        public static string DefaultFindRouteLocationOperationName
        {
            get { return _defaultFindRouteLocationOperationName; }
        }


        /// <summary>
        /// The default URL.
        /// </summary>
        public static string DefaultUrl
        {
            get { return _defaultUrl; }
        }


        private string _url;
        private string _findRouteLocationOperationName;
        private string _findNearestRouteLocationOperationName;
        private string _routesResourceName;

        /// <summary>
        /// The URL to the REST endpoint.
        /// </summary>
        [DataMember]
        public string Url
        {
            get { return string.IsNullOrEmpty(_url) ? _defaultUrl : _url; }
            set { _url = value; }
        }

        /// <summary>
        /// The operation name for "Find Route Locations".
        /// </summary>
        [DataMember]
        public string FindRouteLocationOperationName
        {
            get { return string.IsNullOrEmpty(_findRouteLocationOperationName) ? _defaultFindRouteLocationOperationName : _findRouteLocationOperationName; }
            set { _findRouteLocationOperationName = value; }
        }

        /// <summary>
        /// The operation name for "Find Nearest Route Locations".
        /// </summary>
        [DataMember]
        public string FindNearestRouteLocationOperationName
        {
            get { return string.IsNullOrEmpty(_findNearestRouteLocationOperationName) ? _defaultFindNearestRouteLocationsOperationName : _findNearestRouteLocationOperationName; }
            set { _findNearestRouteLocationOperationName = value; }
        }

        /// <summary>
        /// The operation name for "Routes" resource name.
        /// </summary>
        [DataMember]
        public string RoutesResourceName
        {
            get { return string.IsNullOrEmpty(_routesResourceName) ? _defaultRoutesResourceName : _routesResourceName; }
            set { _routesResourceName = value; }
        }

        /// <summary>
        /// Creates a new ElcSettings instance.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="findRouteLocationOperationName"></param>
        /// <param name="findNearestRouteLocationOperationName"></param>
        /// <param name="routesResourceName"></param>
        public ElcSettings(string url = _defaultUrl, string findRouteLocationOperationName = _defaultFindRouteLocationOperationName,
            string findNearestRouteLocationOperationName = _defaultFindNearestRouteLocationsOperationName,
            string routesResourceName = _defaultRoutesResourceName)
        {
            _url = string.IsNullOrEmpty(url) ? _defaultUrl : url;
            _findRouteLocationOperationName = string.IsNullOrEmpty(findRouteLocationOperationName) ? _defaultFindRouteLocationOperationName : findRouteLocationOperationName;
            _findNearestRouteLocationOperationName = string.IsNullOrEmpty(findNearestRouteLocationOperationName) ? _defaultFindNearestRouteLocationsOperationName : findNearestRouteLocationOperationName;
            _routesResourceName = string.IsNullOrEmpty(routesResourceName) ? _defaultRoutesResourceName : routesResourceName;
        }

        /// <summary>
        /// Compares the <see cref="ElcSettings"/> with another object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj != null || obj.GetType() == typeof(ElcSettings))
            {
                var other = (ElcSettings)obj;
                return string.Equals(Url, other.Url, StringComparison.Ordinal) && string.Equals(FindNearestRouteLocationOperationName, other.FindNearestRouteLocationOperationName, StringComparison.Ordinal) && string.Equals(FindRouteLocationOperationName, other.FindRouteLocationOperationName, StringComparison.Ordinal) && string.Equals(RoutesResourceName, other.RoutesResourceName, StringComparison.Ordinal);
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// Compares this <see cref="ElcSettings"/> with another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Returns <see langword="true"/> if they are equal, <see langword="false"/> otherwise.</returns>
        public bool Equals(ElcSettings other)
        {
            return Equals(other as object);
        }

        /// <summary>
        /// Determines if two <see cref="ElcSettings"/> values are equal.
        /// </summary>
        /// <param name="settings1"></param>
        /// <param name="settings2"></param>
        /// <returns>Returns <see langword="true"/> if they are equal, <see langword="false"/> otherwise.</returns>
        public static bool operator ==(ElcSettings settings1, ElcSettings settings2)
        {
            return settings1.Equals(settings2);
        }

        /// <summary>
        /// Determines if two <see cref="ElcSettings"/> values are not equal.
        /// </summary>
        /// <param name="settings1"></param>
        /// <param name="settings2"></param>
        /// <returns>Returns <see langword="false"/> if they are equal, <see langword="true"/> otherwise.</returns>
        public static bool operator !=(ElcSettings settings1, ElcSettings settings2)
        {
            return !settings1.Equals(settings2);
        }

        /// <summary>
        /// Serves as a hash function for a particluar type.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Url.GetHashCode() ^ FindRouteLocationOperationName.GetHashCode() ^ FindNearestRouteLocationOperationName.GetHashCode() ^ RoutesResourceName.GetHashCode();
        }


    }
}
