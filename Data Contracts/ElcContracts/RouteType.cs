using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Interstate Route
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
        /// Local Access approach (LX) Frontage Roads (FD, FI)
        /// </summary>
        LC = 4,
        /// <summary>
        /// Ferry Terminals
        /// </summary>
        FT = 5,
        /// <summary>
        /// Proposed Routes
        /// </summary>
        PR = 6,
        /// <summary>
        /// Connector - a segment used to connect state routes to other routes/roadways for future use in a geometric network.
        /// </summary>
        CN = 7,
        /// <summary>
        /// Turnback
        /// </summary>
        TB = 8,
    }
}
