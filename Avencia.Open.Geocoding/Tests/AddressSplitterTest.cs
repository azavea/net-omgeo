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

using Avencia.Open.Common;
using NUnit.Framework;

namespace Avencia.Geocoding.Tests
{
    ///<exclude/>
	[TestFixture]
	public class AddressSplitterTest
	{
        ///<exclude/>
        [Test]
		public void Split0()
		{
			const string address = "340 N 12th St, Philadelphia, PA, USA";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("340 N 12th St", req.Address, "Test address doesn't match.");
			Assert.AreEqual("Philadelphia", req.City, "Test city doesn't match.");
			Assert.AreEqual("PA", req.State, "Test state doesn't match.");
			Assert.AreEqual("USA", req.Country, "Test country doesn't match.");
		}

        ///<exclude/>
        [Test]
		public void Split1()
		{
			const string address = "340 N 12th St #402, Philadelphia, PA, USA";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("340 N 12th St #402", req.Address, "Test address doesn't match.");
			Assert.AreEqual("Philadelphia", req.City, "Test city doesn't match.");
			Assert.AreEqual("PA", req.State, "Test state doesn't match.");
			Assert.AreEqual("USA", req.Country, "Test country doesn't match.");
		}

        ///<exclude/>
        [Test]
		public void SplitCrossStreets1()
		{
            const string address = "12th St and Callowhill St, Philadelphia, PA, USA";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("12th St & Callowhill St", req.Address, "Test address doesn't match.");
			Assert.AreEqual("Philadelphia", req.City, "Test city doesn't match.");
			Assert.AreEqual("PA", req.State, "Test state doesn't match.");
			Assert.AreEqual("USA", req.Country, "Test country doesn't match.");
		}

        ///<exclude/>
        [Test]
		public void SplitCrossStreets2()
		{
            const string address = "12th St & Callowhill St, Philadelphia, PA, USA";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("12th St & Callowhill St", req.Address, "Test address doesn't match.");
			Assert.AreEqual("Philadelphia", req.City, "Test city doesn't match.");
			Assert.AreEqual("PA", req.State, "Test state doesn't match.");
			Assert.AreEqual("USA", req.Country, "Test country doesn't match.");
		}

        ///<exclude/>
        [Test]
		public void SplitCrossStreets3()
		{
            const string address = "12th St @ Callowhill St, Philadelphia, PA, USA";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("12th St & Callowhill St", req.Address, "Test address doesn't match.");
			Assert.AreEqual("Philadelphia", req.City, "Test city doesn't match.");
			Assert.AreEqual("PA", req.State, "Test state doesn't match.");
			Assert.AreEqual("USA", req.Country, "Test country doesn't match.");
		}

        ///<exclude/>
        [Test]
		public void SplitCrossStreets4()
		{
            const string address = "12th St \\ Callowhill St, Philadelphia, PA, USA";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("12th St & Callowhill St", req.Address, "Test address doesn't match.");
			Assert.AreEqual("Philadelphia", req.City, "Test city doesn't match.");
			Assert.AreEqual("PA", req.State, "Test state doesn't match.");
			Assert.AreEqual("USA", req.Country, "Test country doesn't match.");
		}

        ///<exclude/>
        [Test]
		public void SplitCrossStreets5()
		{
            const string address = "12th St and Callowhill St, Philadelphia, PA, USA";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("12th St & Callowhill St", req.Address, "Test address doesn't match.");
			Assert.AreEqual("Philadelphia", req.City, "Test city doesn't match.");
			Assert.AreEqual("PA", req.State, "Test state doesn't match.");
			Assert.AreEqual("USA", req.Country, "Test country doesn't match.");
		}

        ///<exclude/>
        [Test]
		public void SplitCrossStreets6()
		{
            const string address = "12th St + Callowhill St, Philadelphia, PA, USA";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("12th St & Callowhill St", req.Address, "Test address doesn't match.");
			Assert.AreEqual("Philadelphia", req.City, "Test city doesn't match.");
			Assert.AreEqual("PA", req.State, "Test state doesn't match.");
			Assert.AreEqual("USA", req.Country, "Test country doesn't match.");
		}

        ///<exclude/>
        [Test]
		public void SplitZip1()
		{
            const string address = "340 N 12th St, 19107";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("340 N 12th St", req.Address, "Test address doesn't match.");
			Assert.AreEqual("19107", req.PostalCode, "Test postal code doesn't match.");
		}

        ///<exclude/>
        [Test]
		public void SplitZip2()
		{
            const string address = "12th St & Callowhill St, 19107";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("12th St & Callowhill St", req.Address, "Test address doesn't match.");
			Assert.AreEqual("19107", req.PostalCode, "Test postal code doesn't match.");
		}

        ///<exclude/>
        [Test]
		public void SplitZip3()
		{
            const string address = "340 N 12th St, Philadelphia, PA 19107";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("340 N 12th St", req.Address, "Test address doesn't match.");
			Assert.AreEqual("19107", req.PostalCode, "Test postal code doesn't match.");
		}

        ///<exclude/>
        [Test]
		public void SplitZip4()
		{
            const string address = "340 N 12th St, Philadelphia, PA 19107, USA";

			GeocodeRequest req = new GeocodeRequest();
			req.TextString = address;

			Config config = new Config("../../Tests/TestConfig.config", "TestConfig");
			Processors.AddressSplitter splitter = new Processors.AddressSplitter(config, "AddressSplitterTest");

			req = splitter.ProcessRequest(req);

			Assert.AreEqual("340 N 12th St", req.Address, "Test address doesn't match.");
			Assert.AreEqual("19107", req.PostalCode, "Test postal code doesn't match.");
			Assert.AreEqual( "USA", req.Country, "Test country doesn't match." );
		}
	}
}
