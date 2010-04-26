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
	public class ScoreNormalizerTest
	{
        private GeocodeResponse _response;
        private Config _config;
        private Processors.ScoreNormalizer _normalizer;
        private GeocodeCandidate _zero, _one, _two;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _config = new Config("../../Tests/TestConfig.config", "TestConfig");
            GeocoderSourceDummy dummy = new GeocoderSourceDummy(_config, "DummyProcessorWithConfig");
            _normalizer = new Processors.ScoreNormalizer(_config, "ScoreNormalizerTest");

            _zero = new GeocodeCandidate
            {
                MatchType = "Locator0"
            };

            _one = new GeocodeCandidate
            {
                MatchType = "Locator1"
            };
            _two = new GeocodeCandidate
            {
                MatchType = "Locator2"
            };
            List<GeocodeCandidate> candidates = new List<GeocodeCandidate>{_zero, _one, _two};

            _response = new GeocodeResponse(candidates, dummy);
        }
        ///<exclude/>
        [Test]
		public void NormalizeInOrder()
		{
            _zero.MatchScore = 100;
            _one.MatchScore = 75;
            _two.MatchScore = 50;

            _normalizer.ProcessResponse(_response);

            Assert.AreEqual(100, _zero.MatchScore);
            Assert.AreEqual(62.5, _one.MatchScore);
            Assert.AreEqual( 37.5, _two.MatchScore);
        }

        [Test]
        public void NormalizeReverseOrder()
        {
            _zero.MatchScore = 25;
            _one.MatchScore = 50;
            _two.MatchScore = 100;

            _normalizer.ProcessResponse(_response);

            Assert.AreEqual(62.5,_zero.MatchScore);
            Assert.AreEqual(50, _one.MatchScore);
            Assert.AreEqual(62.5, _two.MatchScore);
        }

        [Test]
        public void NormalizeMiddleBest()
        {
            _zero.MatchScore = 10;
            _one.MatchScore = 100;
            _two.MatchScore = 60;

            _normalizer.ProcessResponse(_response);

            Assert.AreEqual(55, _zero.MatchScore);
            Assert.AreEqual(75, _one.MatchScore);
            Assert.AreEqual(42.5, _two.MatchScore);
        }

        

	}
}
