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
using NUnit.Framework;

namespace Avencia.Geocoding.Tests
{
    /// <exclude/>
    [TestFixture]
    public class DummyGeocoderTests
    {
        /// <exclude/>
        [Test]
        public void TestLoadDummyGeocoder()
        {
            Config dummyConfig = new Config("../../Tests/TestConfig.config", "TestConfig");
            Geocoder gc = new Geocoder(dummyConfig);

            GeocodeRequest gReq = new GeocodeRequest();
            gReq.Address = "340 N 12th St";
            IList<GeocodeCandidate> candidates = gc.Geocode(gReq).Candidates;
            Assert.AreEqual(1, candidates.Count);
        }

        /// <exclude />
        [Test]
        public void TestLoadDummyGeocoderWithProcessor()
        {
            Config dummyConfig = new Config("../../Tests/TestConfig.config", "TestConfig");
            Geocoder gc = new Geocoder(dummyConfig, "GeocoderSourcesWithProcessors");

            GeocodeRequest gReq = new GeocodeRequest();
            gReq.Address = "tS ht21 N 043";
            IList<GeocodeCandidate> candidates = gc.Geocode(gReq).Candidates;
            Assert.AreEqual(1, candidates.Count);
        }


        /// <exclude />
        [Test]
        public void TestLoadDummyGeocoderWithProcessorWithConfig()
        {
            Config dummyConfig = new Config("../../Tests/TestConfig.config", "TestConfig");
            Geocoder gc = new Geocoder(dummyConfig, "GeocoderSourcesWithProcessorsWithConfigs");

            GeocodeRequest gReq = new GeocodeRequest();
            gReq.Address = "tS ht21 N 043";
            IList<GeocodeCandidate> candidates = gc.Geocode(gReq).Candidates;
            Assert.AreEqual(1, candidates.Count);
        }
    }
}
