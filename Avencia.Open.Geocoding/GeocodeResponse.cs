// Copyright (c) 2004-2010 Avencia, Inc.
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

using System.Collections.Generic;

namespace Avencia.Geocoding
{
    /// <summary>
    /// A response to a GeocodeRequest.  This includes the source that generated
    /// the response, as well as a list of matches from that source.
    /// </summary>
    public class GeocodeResponse
    {
        /// <summary>
        /// The source that generated this response.
        /// </summary>
        public readonly GeocoderSource GeocodeSource;

        /// <summary>
        /// All the candidates that matched the request.  May be empty,
        /// will not be null.
        /// </summary>
        public readonly List<GeocodeCandidate> Candidates;

        /// <summary>
        /// A flag indicating whether all of the parts of the address
        /// were used by the geocoder.  True if all parts of the input address
        /// were passed to the geocoder, and false otherwise.
        /// Currently only used by ArcIMS Shapefile and ArcIMS Route Server 
        /// geocoders (9/24/08).
        /// </summary>
        public readonly bool StrictAddressMatch;

        /// <summary>
        /// Creates a response.
        /// </summary>
        /// <param name="candidates">All the candidates that matched the request.</param>
        /// <param name="geocodeSource">The source that is creating this response.</param>
        public GeocodeResponse(IEnumerable<GeocodeCandidate> candidates, GeocoderSource geocodeSource)
            : this(candidates, geocodeSource, true) { }

        /// <summary>
        /// Creates a response.
        /// </summary>
        /// <param name="candidates">All the candidates that matched the request.</param>
        /// <param name="geocodeSource">The source that is creating this response.</param>
        /// <param name="strictAddressMatch">A flag indicating whether all of the parts of the address
        ///                                  were used by the geocoder.  True if all parts of the input address
        ///                                  were passed to the geocoder, and false otherwise.</param>
        public GeocodeResponse(IEnumerable<GeocodeCandidate> candidates, GeocoderSource geocodeSource, bool strictAddressMatch)
        {
            GeocodeSource = geocodeSource;

            Candidates = candidates != null ? new List<GeocodeCandidate>(candidates)
                : new List<GeocodeCandidate>();
            Candidates.Sort();

            StrictAddressMatch = strictAddressMatch;
        }

        #region Properties

        /// <summary>
        /// True if there were any matches.
        /// </summary>
        public bool HasCandidates
        {
            get { return (Candidates.Count > 0); }
        }

        /// <summary>
        /// How many matches were generated for the request.
        /// </summary>
        public int Count
        {
            get { return Candidates.Count; }
        }

        #endregion
    }
}