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
using System.Reflection;
using Avencia.Open.Common;

namespace Avencia.Open.Geocoding.Processors
{
    /// <summary>
    /// This request processor takes configuration and simply writes
    /// static text into certain fields of the request (e.g., to force
    /// a certain city or state being fed to the geocoder).
    /// </summary>
    public class AddressPartDefaulter : IRequestProcessor
    {
        private readonly Dictionary<string, string> _configParams;

        /// <summary>
        /// Get the config for the fields to be replaced.
        /// </summary>
        /// <param name="config">The config to use.</param>
        /// <param name="component">The component name that defines the replacements.</param>
        public AddressPartDefaulter(Config config, string component)
        {
            _configParams = config.GetParametersAsDictionary(component);
        }

        /// <summary>
        /// Processes a geocode request to set
        /// </summary>
        /// <param name="request">The response object to be modified</param>
        public GeocodeRequest ProcessRequest(GeocodeRequest request)
        {
            GeocodeRequest modifiedRequest = new GeocodeRequest(request);
            const string leader = "default";
            foreach (string key in _configParams.Keys)
            {
                if (key.ToLower().StartsWith(leader))
                {
                    string name = key.Substring(leader.Length);

                    FieldInfo fi = typeof (GeocodeRequest).GetField(name);
                    if (fi != null)
                    {
                        fi.SetValue(modifiedRequest, _configParams[key]);
						continue;
                    }

					PropertyInfo pi = typeof(GeocodeRequest).GetProperty(name);
					if (pi != null)
					{
						pi.SetValue(modifiedRequest, _configParams[key], null);
						continue;
					}
                }
            }
            return modifiedRequest;
        }

        /// <summary>
        /// Exposes the configured settings for uses other than the processor
        /// </summary>
        ///<param name="setting">The setting you are looking to inspect</param>
        ///<returns>The configured default value for the setting</returns>
        public string GetDefault(string setting)
        {
            const string leader = "default";
            foreach (string key in _configParams.Keys)
            {
                if (key.ToLower().Equals(leader + setting.ToLower()))
                {
                    return _configParams[key];
                }
            }

            return "";
        }
    }
}
