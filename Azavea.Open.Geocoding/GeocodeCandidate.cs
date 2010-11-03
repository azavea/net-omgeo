// Copyright (c) 2004-2010 Azavea, Inc.
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;

namespace Azavea.Open.Geocoding
{
    /// <summary>
    /// A single match from geocoding some input.
    /// </summary>
    [Serializable]
    public class GeocodeCandidate : AddressContainer, IComparable
    {
        /// <summary>
        /// How good a match was this candidate.  Higher is better.  Actual range of values
        /// depends on the IGeocoderSource that generated it.
        /// </summary>
        public double MatchScore { get; set; }
        /// <summary>
        /// The type of match used to return this candidate, e.g., Rooftop, Street Level, Zip.
        /// These are called "Locators" in ESRI parlance.
        /// Not all IGeocoderSources will populate this field
        /// </summary>
        public string MatchType { get; set; }
        /// <summary>
        /// Latitude of the result (if in lat/lon, otherwise the Y value of whatever
        /// coordinate system is in use).
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// Longitude of the result (if in lat/lon, otherwise the X value of whatever
        /// coordinate system is in use).
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// The entire standardized address, including city/state/zip if populated
        /// ("340 N 12TH ST, PHILADELPHIA, PA, 19107").
        /// </summary>
        public string StandardizedAddress { get; set; }
        /// <summary>
        /// The raw output from the geocoding process.
        /// May not be populated by all geocode sources.
        /// </summary>
        public string RawData { get; set; }

        /// <exclude/>
        public int CompareTo(object obj)
        {
            if (!(obj is GeocodeCandidate))
            {
                throw new InvalidCastException("GeocodeCandidates can only be compared to geocode candidates!");
            }

            // Sorts candidates in descending order
            return ((GeocodeCandidate)obj).MatchScore.CompareTo(MatchScore);
        }
    }
}