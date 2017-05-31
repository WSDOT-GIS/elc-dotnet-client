using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wsdot.Elc.Contracts
{
    using System;
    using RrtDict = Dictionary<Regex, string>;



    /// <summary>
    /// Provides extension methods for state route identifiers.
    /// </summary>
    public static class StateRouteExtensions
    {
        const string _rrtList = @"AR Alternate Route
CO Couplet
CD Collector Distributor (Decreasing)
CI Collector Distributor (Increasing)
FD Frontage Road (Decreasing)
FI Frontage Road (Increasing)
FS Ferry Ship (Boat)
FT Ferry Terminal
HI Grade-Separated HOV (Increasing)
HD Grade-Separated HOV (Decreasing)
LX Crossroad within Interchange
PR Proposed Route
PU Extension of P ramp
P[1-9] Off Ramp (Increasing)
QU Extension of Q ramp
Q[1-9] On Ramp (Increasing)
RL Reversible Lane
RU Extension of R ramp
R[1-9] Off Ramp (Decreasing)
SP Spur
SU Extension of S ramp
S[1-9] On Ramp (Decreasing)
TB Transitional Turnback
TR Temporary Route
FU Future";

        /// <summary>
        /// Matches a State Route ID and captures the SR, RRT, and RRQ.  Also matches an SR without an RRT or RRQ.
        /// </summary>
        private static readonly Regex _srRegex = new Regex(@"(?<sr>\d{3})(?:(?<rrt>[A-Za-z0-9]{2})(?<rrq>[A-Za-z0-9]{0,6}))?");
        private static readonly Regex _numericRrqRe = new Regex(@"([FC][DI])|(LX)|(RL)|([PQRS][1-9U])", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        private static readonly Regex _mileageRe = new Regex(@"^(\d+)(B)?", RegexOptions.IgnoreCase);

        private static RrtDict _rrtDescriptions;

        /// <summary>
        /// A dictionary keyed by <see cref="Regex"/> that will match an RRT value.
        /// The value of the dictionary is a <see cref="string"/> describing that RRT.
        /// </summary>
        internal static RrtDict RrtDescriptions
        {
            get
            {
                if (_rrtDescriptions == null)
                {
                    _rrtDescriptions = new RrtDict();
                    // This Regex will match an RRT pattern and description separated by a space.
                    // The pattern will be in group 1 and the description in group 2.
                    var splitRe = new Regex(@"^(\S+)\s(.+)$");

                    // Read each line in the resource string and convert into a dictionary.
                    using (var reader = new StringReader(_rrtList))
                    {
                        string line = reader.ReadLine();
                        Match match;
                        string pattern;
                        string description;
                        while (line != null)
                        {
                            match = splitRe.Match(line);
                            if (match.Success)
                            {
                                pattern = match.Groups[1].Value;
                                description = match.Groups[2].Value;
                                _rrtDescriptions.Add(new Regex(pattern), description);
                            }
                            line = reader.ReadLine();
                        }
                    }
                }
                return _rrtDescriptions;
            }
        }


        /// <summary>
        /// Parses a state route identifier into its component SR, RRT, and RRQ components.
        /// </summary>
        /// <param name="srRrtRrq">A state route identifier, including RRT and RRQ info if applicable.</param>
        /// <param name="sr">The SR component of <paramref name="srRrtRrq"/>.</param>
        /// <param name="rrt">The RRT component of <paramref name="srRrtRrq"/>.</param>
        /// <param name="rrq">The RRQ component of <paramref name="srRrtRrq"/>.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="srRrtRrq"/> is a valid state route identifier, <see langword="false"/> otherwise.</returns>
        public static bool TryParse(this string srRrtRrq, out string sr, out string rrt, out string rrq)
        {
            // Initialize out parameters.
            sr = rrt = rrq = null;

            if (srRrtRrq == null)
            {
                return false;
            }

            Match srMatch = _srRegex.Match(srRrtRrq);
            if (!srMatch.Success)
            {
                return false;
            }

            GroupCollection groups = srMatch.Groups;
            sr = groups["sr"].Value;
            rrt = groups["rrt"].Value;
            rrq = groups["rrq"].Value;

            return true;
        }

        /// <summary>
        /// Sorts SR, RRT, and RRQs into groups.
        /// </summary>
        /// <param name="routes">A list of state route identifier strings.</param>
        /// <returns>A grouping of route IDs by SR, then RRT, then RRQ.</returns>
        public static Dictionary<string, Dictionary<string, List<string>>> CategorizeRoutes(this IEnumerable<string> routes)
        {
            var output = new Dictionary<string, Dictionary<string, List<string>>>();
            string sr, rrt, rrq;

            foreach (string route in routes)
            {
                if (route.TryParse(out sr, out rrt, out rrq))
                {
                    // Get the dictionary keyed by SR.
                    Dictionary<string, List<string>> rrtDict;
                    if (output.ContainsKey(sr))
                    {
                        rrtDict = output[sr];
                    }
                    else
                    {
                        rrtDict = new Dictionary<string, List<string>>();
                        output.Add(sr, rrtDict);
                    }

                    // Get the rrq list.
                    List<string> rrqList;
                    if (rrtDict.ContainsKey(rrt))
                    {
                        rrqList = rrtDict[rrt];
                    }
                    else
                    {
                        rrqList = new List<string>();
                        rrtDict.Add(rrt, rrqList);
                    }
                    if (!rrqList.Contains(rrq))
                    {
                        rrqList.Add(rrq);
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Gets a text description for the RRT string.
        /// </summary>
        /// <param name="rrt">An RRT string.</param>
        /// <returns>
        /// If <paramref name="rrt"/> is <see langword="null"/> or an empty string, "Mainline" is returned.
        /// If <paramref name="rrt"/> is one of the RRTs defined in list of valid RRTs
        /// then the corresponding description is returned.
        /// Otherwise <see langword="null"/> is returned.
        /// </returns>
        public static string GetRrtDescription(this string rrt)
        {
            if (string.IsNullOrEmpty(rrt) || string.Compare(rrt, "ML", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Mainline";
            }
            else
            {
                var description = (from k in RrtDescriptions
                                   where k.Key.IsMatch(rrt)
                                   select k.Value).FirstOrDefault();
                return description;
            }
        }

        /// <summary>
        /// Gets a text description of the RRQ of a route.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static string GetRrqDescription(this RouteInfo route)
        {
            float milepostTimes100;
            string output = null;
            var match = _mileageRe.Match(route.Rrq);
            if (match.Success)
            {
                milepostTimes100 = float.Parse(match.Groups[1].Value);
                float milepost = milepostTimes100 / 100;
                output = string.Format("Intersects {0} @ {1:0.00}{2}", route.SR, milepost, match.Groups[2].Value);
            }
            return output;
        }
    }
}