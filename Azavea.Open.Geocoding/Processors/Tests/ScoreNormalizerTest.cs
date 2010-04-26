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
using NUnit.Framework;

namespace Azavea.Open.Geocoding.Tests
{
    ///<exclude/>
	[TestFixture]
	public class MetchTypeSelectorProcessorTest
	{
        private GeocodeResponse _response;
        private Config _config;
        private Processors.MatchTypeSelectorProcessor _typeSelector;
        private GeocodeCandidate _zero, _one, _two, _three;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _config = new Config("../../Tests/TestConfig.config", "TestConfig");

        }
        ///<exclude/>
        [Test]
		public void PitchCityState()
		{
            GeocoderSourceDummy dummy = new GeocoderSourceDummy(_config, "DummyProcessorWithConfig");
            _typeSelector = new Processors.MatchTypeSelectorProcessor(_config, "MatchTypeSelectorProcessorTest");

            _zero = new GeocodeCandidate
            {
                MatchType = "US_Zip4"
            };

            _one = new GeocodeCandidate
            {
                MatchType = "US_CityState"
            };
            _two = new GeocodeCandidate
            {
                MatchType = "US_RoofTops"
            };
            _three = new GeocodeCandidate
            {
                MatchType = "US_RoofTops"
            };
            List<GeocodeCandidate> candidates = new List<GeocodeCandidate> { _zero, _one, _two, _three };

            _response = new GeocodeResponse(candidates, dummy);

            _typeSelector.ProcessResponse(_response);

            Assert.AreEqual(3, _response.Count);
            Assert.Contains(_zero, _response.Candidates);
            Assert.Contains(_two, _response.Candidates);
            Assert.Contains(_three, _response.Candidates);

        }
	}
}
