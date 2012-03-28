extern alias newGeoAPI;
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

using System.Collections.Generic;
using Azavea.Open.Common;
using Azavea.Open.Reprojection;
using newGeoAPI::GeoAPI.CoordinateSystems;

namespace Azavea.Open.Geocoding.Tests
{
    /// <summary>
    /// This is a simple hardcoded geocoder that does not have network or database
    /// dependencies, to be used for testing.
    /// </summary>
    public class GeocoderSourceDummy : GeocoderSource
    {
        /// <exclude/>
        public GeocoderSourceDummy(Config config, string component) : base(config, component)
        {
        }

        ///<exclude/>
        public override ICoordinateSystem CoordinateSystem
        {
            get { return Reprojector.PAStatePlane; }
        }

        /// <exclude/>
        protected override GeocodeResponse InternalGeocode(GeocodeRequest request)
        {
            if (request.Address == "340 N 12th St")
            {
                GeocodeCandidate c = new GeocodeCandidate();
                c.Latitude = 238554;
                c.Longitude = 2694727;
                c.Address = request.Address;
                c.PostalCode = "19107";
                c.Country = "USA";
                return new GeocodeResponse(new[] {c}, this);
            }
            return new GeocodeResponse(new GeocodeCandidate[] {}, this);
        }

        /// <exclude/>
        protected override IList<GeocodeResponse> InternalBatchGeocode(IList<GeocodeRequest> processedRequests)
        {
            List<GeocodeResponse> responses = new List<GeocodeResponse>();
            foreach (GeocodeRequest req in processedRequests)
            {
                responses.Add(InternalGeocode(req));
            }
            return responses;
        }
    }
}
