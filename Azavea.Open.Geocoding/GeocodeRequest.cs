extern alias newGeoAPI;
﻿// Copyright (c) 2004-2010 Azavea, Inc.
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

using newGeoAPI::GeoAPI.CoordinateSystems;

namespace Azavea.Open.Geocoding
{
    /// <summary>
    /// A request that something be geocoded.  Not all geocoders will use all fields.
    /// </summary>
    public class GeocodeRequest : AddressContainer
    {
		/// <summary>
		/// If you do not know what type of thing you're trying to geocode (address,
		/// place name, intersection, etc) it can be put in here.  All geocoders should
		/// use at least this field.
		/// </summary>
		public string TextString { get; set; }

		/// <summary>
		/// Specify the response coordinate system.  If this is not used, then the
		/// respnse will be returned in the system used by whatever geocoder was used.
		/// </summary>
		public ICoordinateSystem CoordinateSystem { get; set; }

		/// <summary>
		/// Create a new GeocodeRequest
		/// </summary>
		public GeocodeRequest() {}

		/// <summary>
		/// Clone an existing geocode request.
		/// </summary>
		/// <param name="original"></param>
		public GeocodeRequest(GeocodeRequest original)
			: base(original)
		{
			TextString = original.TextString;
			CoordinateSystem = original.CoordinateSystem;			
		}
    }
}