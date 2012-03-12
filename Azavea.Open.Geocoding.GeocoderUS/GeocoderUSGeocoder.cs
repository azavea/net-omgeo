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
using System.Collections.Generic;
using System.Web;
using Azavea.Open.Common;
using Azavea.Open.Geocoding.Util;
using Azavea.Open.Reprojection;
using GeoAPI.CoordinateSystems;

namespace Azavea.Open.Geocoding.GeocoderUS
{
    /// <summary>
    /// GeocoderUS will geocode addresses or intersections
    /// Intersections return empty addresses, though they are geocoded.
    /// There is no match score possible, as GeocoderUS does not provide any quality ratings.
    /// </summary>
    public class GeocoderUSGeocoder : GeocoderSource
    {
        private readonly string _username = "";
        private readonly string _password = "";

        /// <summary>
        /// Creates the geocoder. If the username and password are not set, then the publicly
        /// available interface will be used. This should only be used in *non-commercial* products.
        /// </summary>
        /// <param name="config">The config file we are using.</param>
        /// <param name="sectionName">The config section we are using to look for RequestProcessors and ResponseProcessors.</param>
        public GeocoderUSGeocoder(Config config, string sectionName)
            : base(config, sectionName)
        {
            _username = config.GetParameter(sectionName, "UserName");
            _password = config.GetParameter(sectionName, "Password");
        }

        /// <summary>
        /// This is where actual source-specific geocoding happens.  It is called internal because
        /// clients use <code>Geocode</code> which sandwiches this call between pre- and post-processing.
        /// </summary>
        /// <param name="request">The request to be geocoded.</param>
        /// <returns>A <code>GeocodeResponse</code> describing the results of the geocode.</returns>
        protected override GeocodeResponse InternalGeocode(GeocodeRequest request)
        {
            string geocodeURL;

            if (_username.Length > 0) //authenticated access
            {
                geocodeURL = "http://" + _username + ":" + _password + "@geocoder.us/member/service/namedcsv/geocode?address=";
            }
            else //non-authenticated access
            {
                geocodeURL = "http://geocoder.us/service/namedcsv/geocode?address=";
            }

            string queryString;
            if (!string.IsNullOrEmpty(request.TextString))
            {
                queryString = request.TextString;
            }
            else
            {
                queryString = request.Address + ", " +
                              request.City + ", " + request.State + " " +
                              request.PostalCode + ", " + request.Country;
            }

            //Geocoder US doesn't understand '&'
            queryString = queryString.Replace("&", " and ");
            queryString = HttpUtility.UrlEncode(queryString);
            string response = InternetSourceUtil.GetRawTextFromURL(geocodeURL + queryString);
            IList<GeocodeCandidate> responseList = CSVList2GeocodeCandidates(response);
            return (new GeocodeResponse(responseList, this));
        }

        /// <summary>
        /// Returns the coordinate this geocoder used.
        /// </summary>
        public override ICoordinateSystem CoordinateSystem
        {
            get { return Reprojector.WGS84; }
        }

        private static IList<GeocodeCandidate> CSVList2GeocodeCandidates(string csvList)
        {
            IList<GeocodeCandidate> candidates = new List<GeocodeCandidate>();
            string[] strSplit = new string[1];
            strSplit[0] = "\r\n";

            string[] strCandidates = csvList.Split(strSplit, StringSplitOptions.RemoveEmptyEntries);
            if (strCandidates.Length == 0) return candidates;

            //check for unfound address
            if (strCandidates[0].Contains("couldn't find this address")) return candidates;

            foreach (string strCandidate in strCandidates)
            {
                string[] splitCandidate = strCandidate.Split(',');
                GeocodeCandidate curCandidate = new GeocodeCandidate();

                string[] parse = splitCandidate[0].Split('=');
                curCandidate.Latitude = Convert.ToDouble(parse[1]);

                parse = splitCandidate[1].Split('=');
                curCandidate.Longitude = Convert.ToDouble(parse[1]);

                curCandidate.Address = "";
                parse = splitCandidate[2].Split('=');
                if (parse.Length > 1)
                {
                    curCandidate.Address += parse[1] + " ";
                }

                parse = splitCandidate[3].Split('=');
                if (parse.Length > 1)
                {
                    curCandidate.Address += parse[1] + " ";
                }

                parse = splitCandidate[4].Split('=');
                if (parse.Length > 1)
                {
                    curCandidate.Address += parse[1] + " ";
                }

                parse = splitCandidate[5].Split('=');
                if (parse.Length > 1)
                {
                    curCandidate.Address += parse[1] + " ";
                }

                parse = splitCandidate[6].Split('=');
                if (parse.Length > 1)
                {
                    curCandidate.Address += parse[1] + " ";
                }

                //Remove trailing space
                if (curCandidate.Address.Length > 0) curCandidate.Address = curCandidate.Address.Remove(curCandidate.Address.Length - 1, 1);

                parse = splitCandidate[7].Split('=');
                if (parse.Length > 1)
                {
                    curCandidate.City = parse[1];
                }

                parse = splitCandidate[8].Split('=');
                if (parse.Length > 1)
                {
                    curCandidate.State = parse[1];
                }

                parse = splitCandidate[9].Split('=');
                if (parse.Length > 1)
                {
                    curCandidate.PostalCode = parse[1];
                }

                curCandidate.RawData = strCandidate;
                curCandidate.StandardizedAddress = curCandidate.Address + ", " + curCandidate.City + " " + curCandidate.State + ", " + curCandidate.PostalCode;
                candidates.Add(curCandidate);
            }
            return candidates;
        }
    }
}
