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
using Avencia.Open.Common;

namespace Avencia.Open.Geocoding.Processors
{
    /// <summary>
    /// This process examines a list of applicable countries.  If the request's country is
    /// found in that list, the request is returned as-is for processing.  If the country
    /// is not found, a blank request is returned, indicating the the geocoder should not
    /// attempt to geocode this request
    /// </summary>
    public class CountrySelectorProcessor : IRequestProcessor
    {
        private readonly List<string> _myCountries;

        /// <summary>
        /// Create a new Processor with the given configuration
        /// </summary>
        /// <param name="config">An Avencia.Open Config object</param>
        /// <param name="sectionName">The section with the country list required by this processor</param>
        public CountrySelectorProcessor(Config config, string sectionName)
        {
            string myCountryList = config.GetParameter(sectionName, "CountryList");
            if (myCountryList != null)
            {
                _myCountries = new List<string>(myCountryList.Split('|'));
            }
            else throw new LoggingException("No country list in this config");
        }

        /// <summary>
        /// Implementations of this method will be used to process a geocode request, to
        /// chunk a text address into its individua parts, for example.
        /// </summary>
        /// <param name="request">The response object to be modified.</param>
        public GeocodeRequest ProcessRequest(GeocodeRequest request)
        {
            // If the country list doesn't have this request's country, return an empty request
            if (request.Country != null && !_myCountries.Contains(request.Country.ToUpper()))
            {
                return new GeocodeRequest();
            }
            // Else return the original request for geocoding
            return request;
        }
    }
}