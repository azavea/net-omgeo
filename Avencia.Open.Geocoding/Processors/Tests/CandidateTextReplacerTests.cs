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
using System.Xml;
using Avencia.Open.Common;
using NUnit.Framework;

namespace Avencia.Geocoding.Processors.Tests
{
    /// <exclude />
    [TestFixture]
    public class CandidateTextReplacerTests
    {
        /// <exclude />
        [Test]
        public void TestReplacement()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <components>
                <component name='Geocoder'>
                    <parameter name='DummyGeocoder' value='Avencia.Geocoding.Tests.GeocoderSourceDummy,Avencia.Geocoding' />
                </component>

                <component name='DummyGeocoder'>
                    <parameter name='CandidateTextReplacer' value='Avencia.Geocoding.Processors.CandidateTextReplacer,Avencia.Geocoding' />
                </component>

                <component name='CandidateTextReplacer'>
                    <parameter name='ReplaceField' value='Address' />
                    <parameter name='Find' value='340' />
                    <parameter name='ReplaceWith' value='680' />
                </component>
            </components>
            ");
            Config gcCfg = new Config("GcConfig", doc);
            Geocoder gc = new Geocoder(gcCfg, "Geocoder");

            GeocodeRequest gReq = new GeocodeRequest();
            gReq.Address = "340 N 12th St";
            IList<GeocodeCandidate> candidates = gc.Geocode(gReq).Candidates;
            Assert.AreEqual(1, candidates.Count);
            Assert.AreEqual("680 N 12th St", candidates[0].Address);
        }

        /// <exclude />
        [Test]
        public void TestReplacementWithExpansion()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <components>
                <component name='Geocoder'>
                    <parameter name='DummyGeocoder' value='Avencia.Geocoding.Tests.GeocoderSourceDummy,Avencia.Geocoding' />
                </component>

                <component name='DummyGeocoder'>
                    <parameter name='CandidateTextReplacer' value='Avencia.Geocoding.Processors.CandidateTextReplacer,Avencia.Geocoding' />
                </component>

                <component name='CandidateTextReplacer'>
                    <parameter name='ReplaceField' value='StandardizedAddress' />
                    <parameter name='Find' value='^' />
                    <parameter name='ReplaceWith' value='{Address}, {PostalCode}, {Country}' />
                </component>
            </components>
            ");
            Config gcCfg = new Config("GcConfig", doc);
            Geocoder gc = new Geocoder(gcCfg, "Geocoder");

            GeocodeRequest gReq = new GeocodeRequest();
            gReq.Address = "340 N 12th St";
            IList<GeocodeCandidate> candidates = gc.Geocode(gReq).Candidates;
            Assert.AreEqual(1, candidates.Count);
            Assert.AreEqual("340 N 12th St, 19107, USA", candidates[0].StandardizedAddress);
        }
    }
}
