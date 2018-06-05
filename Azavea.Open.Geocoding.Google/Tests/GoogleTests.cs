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
using Azavea.Open.Reprojection;
using GisSharpBlog.NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Azavea.Open.Geocoding.Google.Tests
{
    ///<exclude/>
    [TestFixture]
    public class GoogleTests
    {
        private readonly Geocoder _googleGeocoder = new Geocoder(
            new Config("../../Tests/GoogleTests.config", "GoogleTest"),
            "GoogleTestSources");

        ///<exclude/>
        [Test]
        public void TestGoogleGeocoderInvoke()
        {
            Console.WriteLine("Test - Google Geocoder Invoke");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "1 main st., Burlington, vt";

            GeocodeResponse gRes = _googleGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
            Assert.IsTrue(gRes.Candidates.Count == 1, "Expected the geocoder to return 3 results");

            Assert.AreEqual("premise", gRes.Candidates[0].MatchType, "Expected the first match to have the type 'premise'");
        }

        ///<exclude/>
        [Test]
        public void TestGoogleGeocoderHeirarchyProblemAddress()
        {
            Console.WriteLine("Test - Google Heirarchy Problem Address");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "118 Blakely Rd at Lakeshore Drive Colchester VT US ";

            GeocodeResponse gRes = _googleGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
            Assert.IsTrue(gRes.HasCandidates, "Google Geocoder returned no responses");
        }

        ///<exclude/>
        [Test]
        public void TestGoogleGeocoderCityOnly()
        {
            Console.WriteLine("Test - Google Geocoder City Only");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "Burlington VT US";

            GeocodeResponse gRes = _googleGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
            Assert.IsTrue(gRes.HasCandidates, "Google Geocoder returned no responses");
            Assert.AreEqual("locality", gRes.Candidates[0].MatchType, "Google Geocoder returned wrong MatchType for locality");
        }

        ///<exclude/>
        [Test]
        public void TestGoogleGeocoderInvokeNotText()
        {
            Console.WriteLine("Test - Google Geocoder Invoke Not Text");
            GeocodeRequest gr = new GeocodeRequest();
            gr.Address = "340 N 12th St.";
            gr.City = "Philadelphia";
            gr.State = "PA";

            GeocodeResponse gRes = _googleGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
            Assert.IsTrue(gRes.HasCandidates, "Google Geocoder returned no responses");
        }

        ///<exclude/>
        [Test]
        public void TestGoogleGeocoderInvokeMultipleResponse()
        {
            Console.WriteLine("Test - Google Geocoder Invoke Multiple Response");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "600 12th St., Philadelphia PA";

            GeocodeResponse gRes = _googleGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
            Assert.Greater(gRes.Count, 1, "Google Geocoder returned fewer than 2 responses");
        }

        ///<exclude/>
        [Test]
        public void TestGoogleGeocoderAmbiguousAddress()
        {
            Console.WriteLine("Test - Google Geocoder, Ambiguous Address");
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "212 1st ave, Philadelphia, PA";

            GeocodeResponse gRes = _googleGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
        }

        ///<exclude/>
        [Test]
        public void TestGoogleGeocoderAmpersandInAddress()
        {
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "15th & Tasker Sts, Philadelphia, PA";
            GeocodeResponse gRes = _googleGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);

            Assert.AreEqual(1, gRes.Candidates.Count);
            Assert.AreEqual("Tasker St & S 15th St, Philadelphia, PA 19146, USA", gRes.Candidates[0].StandardizedAddress, "Geocoder found wrong intersection");
        }
        
        ///<exclude/>
        [Test]
        public void TestGoogleGeocoderAmpersandInAddress2()
        {
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "21st & Cherry Sts, Philadelphia, PA";
            GeocodeResponse gRes = _googleGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);

            Assert.AreEqual(1, gRes.Candidates.Count);
            Assert.AreEqual("N 21st St & Cherry St, Philadelphia, PA 19103, USA", gRes.Candidates[0].StandardizedAddress, "Geocoder found wrong intersection");
        }

        ///<exclude/>
        [Test]
        public void TestReprojection()
        {
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "340 North 12th St, Philadelphia, PA";
            gr.CoordinateSystem = Reprojector.PAStatePlane;
            GeocodeResponse response = _googleGeocoder.Geocode(gr);
            Assert.Greater(response.Candidates.Count, 0);
            Assert.IsTrue(
                new Point(2694727, 238554).Buffer(200).Contains(
                    new Point(response.Candidates[0].Longitude, response.Candidates[0].Latitude)
                    )
                );
        }

        ///<exclude/>
        [Test]
        public void TestIntersection()
        {
            GeocodeRequest gr = new GeocodeRequest();
            gr.TextString = "12th st and callowhill, philadelphia, pa";

            GeocodeResponse gRes = _googleGeocoder.Geocode(gr);
            TestUtils.OutputGeocodeResponses(gRes);
            Assert.AreEqual(1, gRes.Candidates.Count, "Expected the geocoder to return 1 result");
            Assert.AreEqual("intersection", gRes.Candidates[0].MatchType, "Expected the match type to be 'intersections'");
            Assert.AreEqual("N 12th St & Callowhill St, Philadelphia, PA 19123, USA", gRes.Candidates[0].StandardizedAddress);
        }
    }
}