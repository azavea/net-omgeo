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
using System.Xml;
using Azavea.Open.Common;
using NUnit.Framework;

namespace Azavea.Open.Geocoding.Processors.Tests
{
    /// <exclude />
    [TestFixture]
    public class SpecialCharacterRemoverTest
    {
        /// <exclude />
        [Test]
        public void TestRemoval()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <components>
                <component name='Geocoder'>
                    <parameter name='DummyGeocoder' value='Azavea.Open.Geocoding.Tests.GeocoderSourceDummy,Azavea.Open.Geocoding' />
                </component>
                <component name='DummyGeocoder'>
                    <parameter name='SpecialCharacterRemover' value='Azavea.Open.Geocoding.Processors.SpecialCharacterRemover,Azavea.Open.Geocoding' />
                </component>
                <component name='SpecialCharacterRemover'>
                </component>
            </components>
            ");
            Config gcCfg = new Config("GcConfig", doc);
            SpecialCharacterRemover remover = new SpecialCharacterRemover(gcCfg, "SpecialCharacterRemoverTest");

            GeocodeRequest gReq = new GeocodeRequest();
            gReq.Address = "230 No%rtheast @1st$ St.";
            gReq.City = "O*klahoma? (City)";
            gReq.PostalCode = "[73104]";
            gReq.State = "OK!";
            gReq.Country = "US@";

            GeocodeRequest processed = remover.ProcessRequest(gReq);
            Assert.AreEqual("Oklahoma City", processed.City);
            Assert.AreEqual("230 Northeast 1st St.", processed.Address);
            Assert.AreEqual("73104", processed.PostalCode);
            Assert.AreEqual("OK", processed.State);
            Assert.AreEqual("US", processed.Country);
        }

        /// <exclude />
        [Test]
        public void TestRemovalWithConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <components>
                <component name='Geocoder'>
                    <parameter name='DummyGeocoder' value='Azavea.Open.Geocoding.Tests.GeocoderSourceDummy,Azavea.Open.Geocoding' />
                </component>

                <component name='DummyGeocoder'>
                    <parameter name='SpecialCharacterRemover' value='Azavea.Open.Geocoding.Processors.SpecialCharacterRemover,Azavea.Open.Geocoding' />
                </component>

                <component name='SpecialCharacterRemover'>
                    <parameter name='CharactersToRemove' value='|' />
                </component>
            </components>
            ");
            Config gcCfg = new Config("GcConfig", doc);
            SpecialCharacterRemover remover = new SpecialCharacterRemover(gcCfg, "SpecialCharacterRemover");

            GeocodeRequest gReq = new GeocodeRequest();
            gReq.TextString = "14th @ Impossible | Philadelphia | PA | 19107";
            GeocodeRequest processed = remover.ProcessRequest(gReq);
            Assert.AreEqual("14th @ Impossible  Philadelphia  PA  19107", processed.TextString);
        }
    }
}
