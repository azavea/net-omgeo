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

using System;
using Avencia.Open.Geocoding.Tests;
using Avencia.Open.Common;
using NUnit.Framework;

namespace Avencia.Open.Geocoding.Yahoo.Tests
{
    ///<exclude/>
    [TestFixture]
    public class YahooTests
    {
        private readonly Geocoder _yahooGeocoder = new Geocoder(
            new Config("../../Tests/YahooTests.config", "YahooTest"),
            "YahooTestSources");

        ///<exclude/>
        [Test]
        public void TestYahooGeocoderInvoke()
        {
            Console.WriteLine("Test - Yahoo Geocoder Invoke");
            GeocodeRequest gr = new GeocodeRequest();
            gr.Address = "1 Main st.";
            gr.City = "Burlington";
            gr.State = "VT";
            gr.PostalCode = "05401";

            GeocodeResponse gRes = _yahooGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
            Assert.IsTrue(gRes.HasCandidates, "Yahoo Geocoder returned no responses");
        }

        ///<exclude/>
        [Test]
        public void TestYahooGeocoderTextString()
        {
            Console.WriteLine("Test - Yahoo Geocoder TextString");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "340 N. 12th st., Philadelphia,PA, 19107";

            GeocodeResponse gRes = _yahooGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
            Assert.IsTrue(gRes.HasCandidates, "Yahoo Geocoder returned no responses");
        }

        ///<exclude/>
        [Test]
        public void TestYahooGeocoderAmbiguousAddress()
        {
            Console.WriteLine("Test - Yahoo Geocoder, Ambiguous Address");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "212 1st ave, Philadelphia, PA";

            GeocodeResponse gRes = _yahooGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
        }

        ///<exclude/>
        [Test]
        public void TestYahooGeocoderIntersection()
        {
            Console.WriteLine("Test - Yahoo Geocoder, Intersection");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "Main & N. Winooski, Burlington, VT";

            GeocodeResponse gRes = _yahooGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
        }
    }
}