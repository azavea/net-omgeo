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
using System.Reflection;
using System.Text.RegularExpressions;
using Azavea.Open.Common;

namespace Azavea.Open.Geocoding.Processors
{
	/// <summary>
	/// A processor that does request and response processing of addresses.
	/// </summary>
    public class AddressSplitter : IRequestProcessor, IResponseProcessor
    {
		private readonly Dictionary<string, string> _configParams;

		/// <summary>
		/// Get the config for the regular expressions to split.
		/// </summary>
		/// <param name="config">The config file to use.</param>
		/// <param name="component">The component name that defines the regular expressions.</param>
		public AddressSplitter(Config config, string component)
		{
			_configParams = config.GetParametersAsDictionary(component);
		}

		/// <summary>
		/// Parse the request TextString into address parts.
		/// </summary>
		/// <param name="request">A geocode request.</param>
		/// <returns>A modified geocode request.</returns>
		public GeocodeRequest ProcessRequest(GeocodeRequest request)
		{
			return Parse(request, request.TextString);
		}

		/// <summary>
		/// Parse the response StandardizedAddress into address parts.
		/// </summary>
		/// <param name="response">A geocode response.</param>
		public void ProcessResponse(GeocodeResponse response)
		{
			for (int i = 0; i < response.Candidates.Count; i++)
			{
				GeocodeCandidate candidate = response.Candidates[i];

				candidate = Parse(candidate, candidate.StandardizedAddress);

				response.Candidates[i] = candidate;
			}
		}

		/// <summary>
		/// Parse a string into an address container.
		/// </summary>
		/// <typeparam name="T">A container of properties such as Address, City, State, etc.</typeparam>
		/// <param name="container">A recepticle for address parts.</param>
		/// <param name="address">A string representation of an address.</param>
		/// <returns>A container with fields set from the string.</returns>
		private T Parse<T>(T container, string address) where T : AddressContainer, new()
		{
			T modified = container.Clone() as T;

            if (address == null) return modified;

			foreach (string key in _configParams.Keys)
			{
				if (key.ToLower().EndsWith("re"))
				{
					Regex re = new Regex(_configParams[key].Trim(), RegexOptions.IgnoreCase);

					if (re.IsMatch(address))
					{
						Match m = re.Match(address);
						GroupCollection gc = m.Groups;

						PropertyInfo[] fis = typeof(T).GetProperties();
						foreach (PropertyInfo fi in fis)
						{
							if (gc[fi.Name].Captures.Count == 1)
								fi.SetValue(modified, gc[fi.Name].Value.Trim(), null);
							else if (gc[fi.Name].Captures.Count > 1 &&
								_configParams.ContainsKey(fi.Name + "Concat"))
							{
								string whole = "";
								string concat = _configParams[fi.Name + "Concat"];

								foreach (Capture c in gc[fi.Name].Captures)
								{
									whole += c.Value.Trim() + concat;
								}
								if (whole.Length > concat.Length)
									whole = whole.Remove(whole.Length - concat.Length);

								fi.SetValue(modified, whole, null);
							}
						}

						break;
					}
				}
			}

			return modified;
		}
	}
}
