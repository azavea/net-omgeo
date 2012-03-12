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
using System.Text;
using Azavea.Open.Common;
using Azavea.Open.Geocoding.Processors;

namespace Azavea.Open.Geocoding.Tests
{
    /// <exclude />
    public class RequestProcessorDummyWithConfig : IRequestProcessor
    {
        private readonly bool _doReversing;

        /// <exclude />
        public RequestProcessorDummyWithConfig(Config config, string component)
        {
            _doReversing = Convert.ToBoolean(config.GetParameter(component, "DoReversing", "false"));
        }

        ///<exclude/>
        public GeocodeRequest ProcessRequest(GeocodeRequest request)
        {
			GeocodeRequest modifiedRequest = new GeocodeRequest(request);
            if (_doReversing)
            {
                // This dummy preprocessor just reverses the address text.  Very sneaky.
                StringBuilder sb = new StringBuilder();
                for (int i = request.Address.Length - 1; i >= 0; i--)
                {
                    sb.Append(request.Address[i]);
                }
                modifiedRequest.Address = sb.ToString();
            }
        	return modifiedRequest;
        }
    }
}
