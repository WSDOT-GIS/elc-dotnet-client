using System;
using System.Collections.Generic;
using System.Text;

namespace Wsdot.Elc.Contracts
{
    public enum RouteType
    {
        /// <summary>
        /// State Route
        /// </summary>
        SR = 0,
        /// <summary>
        /// Interstate
        /// </summary>
        IS = 1,
        /// <summary>
        /// US Route
        /// </summary>
        US = 2,
        /// <summary>
        /// Ramp
        /// </summary>
        RA = 3,
        /// <summary>
        /// Local Connector
        /// </summary>
        LC = 4,
        /// <summary>
        /// Ferry Terminal
        /// </summary>
        FT = 5,
        /// <summary>
        /// Proposed Route
        /// </summary>
        PR = 6,
        /// <summary>
        /// ?
        /// </summary>
        CN = 7,
        /// <summary>
        /// Turnback
        /// </summary>
        TB = 8,
    }
}
