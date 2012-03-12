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
using Azavea.Open.Geocoding.Tests;
using Azavea.Open.Common;
using NUnit.Framework;

namespace Azavea.Open.Geocoding.GeocoderUS.Tests
{
    ///<exclude/>
    [TestFixture]
    public class GeocoderUSTests
    {
        private readonly Geocoder _geocoderUS = new Geocoder(
            new Config("../../Tests/GeocoderUSTests.config", "GeocoderUSTest"),
            "GeocoderUSTestSources");

        ///<exclude/>
        [Test]
        public void TestGeocoderUSInvoke()
        {
            Console.WriteLine("Test - GeocoderUS Invoke");
            GeocodeRequest gr = new GeocodeRequest();
            gr.Address = "1 Main st.";
            gr.City = "Burlington";
            gr.State = "VT";
            gr.PostalCode = "05401";

            GeocodeResponse gRes = _geocoderUS.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
            Assert.IsTrue(gRes.HasCandidates, "Geocoder US geocoder returned no responses");
        }

        ///<exclude/>
        [Test]
        public void TestGeocoderUSTextString()
        {
            Console.WriteLine("Test - GeocoderUS TextString");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "340 N. 12th st., Philadelphia,PA, 19107";

            GeocodeResponse gRes = _geocoderUS.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
            Assert.IsTrue(gRes.HasCandidates, "Geocoder US geocoder returned no responses");
        }

        ///<exclude/>
        [Test]
        public void TestGeocoderUSAmbiguousAddress()
        {
            Console.WriteLine("Test - Geocoder US geocoder, Ambiguous Address");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "340 12th St., Philadelphia,PA, 19107";

            GeocodeResponse gRes = _geocoderUS.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
        }

        ///<exclude/>
        [Test]
        public void TestGeocoderUSNonsenseAddress()
        {
            Console.WriteLine("Test - Geocoder US geocoder, Nonsense Address");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "2554545 N. Wheaton St., Burlington VT 05401";

            GeocodeResponse gRes = _geocoderUS.Geocode(gr);

            Assert.IsTrue(gRes.Count == 0, "This should not return any hits!");
            TestUtils.OutputGeocodeResponses(gRes);
        }

        ///<exclude/>
        [Test]
        public void TestGeocoderUSIntersection()
        {
            Console.WriteLine("Test - Geocoder US geocoder, Intersection");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "Main & N. Winooski, Burlington, VT";

            GeocodeResponse gRes = _geocoderUS.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
        }
    }
}
