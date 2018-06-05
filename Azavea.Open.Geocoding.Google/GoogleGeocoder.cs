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
using System.Xml;
using Azavea.Open.Common;
using Azavea.Open.Geocoding.Util;
using Azavea.Open.Reprojection;
using GeoAPI.CoordinateSystems;

namespace Azavea.Open.Geocoding.Google
{
    /// <summary>
    /// Geocoder source for Google Geocoder.
    /// </summary>
    public class GoogleGeocoder : GeocoderSource
    {
        private readonly string _authKey;

        /// <summary>
        /// Constructor for the Google geocoder source.
        /// </summary>
        /// <param name="config">The config to use to construct the Google geocoder.</param>
        /// <param name="component">The component in the config in which to look for 
        /// the configuration parameters.</param>
        public GoogleGeocoder(Config config, string component)
            : base(config, component)
        {
            _authKey = config.GetParameter(component, "GoogleApiKey");
        }

        /// <summary>
        /// This is where actual source-specific geocoding happens.  It is called internal because
        /// clients use <code>Geocode</code> which sandwiches this call between pre- and post-processing.
        /// </summary>
        /// <param name="geocodeRequest">The request to be geocoded.</param>
        /// <returns>A <code>GeocodeResponse</code> describing the results of the geocode.</returns>
        protected override GeocodeResponse InternalGeocode(GeocodeRequest geocodeRequest)
        {
            const string baseGoogleGeocodeUrl = @"https://maps.googleapis.com/maps/api/geocode/xml";

            // For the address, either use the address field or the text string, 
            // depending on which one has a value.  Then, add each following address
            // part, one by one, if it has a value.
            string address =
                (!String.IsNullOrEmpty(geocodeRequest.TextString) ? geocodeRequest.TextString : geocodeRequest.Address) +
                (!String.IsNullOrEmpty(geocodeRequest.City) ? (", " + geocodeRequest.City) : "") +
                (!String.IsNullOrEmpty(geocodeRequest.State) ? (", " + geocodeRequest.State) : "") +
                (!String.IsNullOrEmpty(geocodeRequest.PostalCode) ? (" " + geocodeRequest.PostalCode) : "") +
                (!String.IsNullOrEmpty(geocodeRequest.Country) ? (", " + geocodeRequest.Country) : "");

            // add oe=utf-8 here for returning valid utf-8, not iso-8859-1
            string googleGeocodeUrl = baseGoogleGeocodeUrl +
                                      "?address=" + HttpUtility.UrlEncode(address) +
                                      "&sensor=false" +
                                      "&key=" + _authKey;

            string response = InternetSourceUtil.GetRawHTMLFromURL(googleGeocodeUrl);
            var responseList = XMLList2GeocodeCandidates(response);
            return (new GeocodeResponse(responseList, this));
        }

        /// <summary>
        /// The coordinate system used by this geocoder.  For Google geocoder, results
        /// are returned in WGS84.
        /// </summary>
        public override ICoordinateSystem CoordinateSystem
        {
            get { return Reprojector.WGS84; }
        }

        private static IList<GeocodeCandidate> XMLList2GeocodeCandidates(string xmlList)
        {
            IList<GeocodeCandidate> candidates = new List<GeocodeCandidate>();
            var doc = new XmlDocument();
            doc.LoadXml(xmlList);
            var results = doc.SelectNodes("//GeocodeResponse/result");

            if (results != null)
            {
                //Parse XML to get geocode candidate data;
                foreach (XmlNode hit in results)
                {
                    candidates.Add(XMLCandidate2GeocodeCandidate(hit));
                }
            }
            return candidates;
        }

        private static string ChildNodeText(XmlNode node, string tagName)
        {
            var element = (XmlElement) node.SelectSingleNode("descendant::" + tagName);
            return element != null ? element.InnerText : null;
        }

        /// <summary>
        /// Create a score for the geocoder match.
        /// </summary>
        /// <param name="XMLCandidate">An XML 'result' node from a Google Geocoder v3 response.</param>
        /// <returns>The score of the match</returns>
        /// <remarks>The v2 Geocoder returned an "accuracy" parameter that used to be retuned as the score.
        /// I have roughly mapped the match type to this old numeric scale:
        /// 
        /// 0    Unknown location. 
        /// 1    Country level accuracy. 
        /// 2    Region (state, province, prefecture, etc.) level accuracy. 
        /// 3    Sub-region (county, municipality, etc.) level accuracy. 
        /// 4    Town (city, village) level accuracy. 
        /// 5    Post code (zip code) level accuracy. 
        /// 6    Street level accuracy. 
        /// 7    Intersection level accuracy. 
        /// 8    Address level accuracy. 
        /// 9    Premise (building name, etc) level accuracy
        /// 
        /// I weighted street_address over point_of_interest because this library primarily targeted at
        /// geocoding street addresses.
        ///</remarks>
        private static double ScoreCandidate(XmlNode XMLCandidate)
        {
            var matchType = ChildNodeText(XMLCandidate, "type");
            double score;
            switch (matchType)
            {
                case "street_address": 
                    score = 8.1;
                    break;
                case "intersection":
                    score = 7;
                    break;
                case "premise":
                    score = 9;
                    break;
                case "point_of_interest":
                    score = 8;
                    break;
                default:
                    score = 6;
                    break;
            }
            return score;
        } 

        private static GeocodeCandidate XMLCandidate2GeocodeCandidate(XmlNode XMLCandidate)
        {
            var candidate = new GeocodeCandidate
                {
                    RawData = XMLCandidate.InnerXml,
                    MatchType = ChildNodeText(XMLCandidate, "type"),
                    StandardizedAddress = ChildNodeText(XMLCandidate, "formatted_address")
                };
            switch (candidate.MatchType)
            {
                case "street_address":
                case "premise":
                case "intersection":
                case "route":
                    candidate.Address = candidate.StandardizedAddress.Split(',')[0];
                    break;
                case "point_of_interest":
                    // Points of interest look like this
                    //   Union Station, 1 Main Street, Burlington, VT 05401, USA
                    // We want the second CSV value
                    candidate.Address = candidate.StandardizedAddress.Split(',')[1];
                    break;
                default:
                    candidate.Address = "";
                    break;
            }
            var locationNode = XMLCandidate.SelectSingleNode("//location");
            if (locationNode != null)
            {
                candidate.Longitude = Convert.ToDouble(ChildNodeText(locationNode, "lng"));
                candidate.Latitude = Convert.ToDouble(ChildNodeText(locationNode, "lat"));
            }
            var componentNodes = XMLCandidate.SelectNodes("//address_component");
            if (componentNodes != null)
            {
                foreach (XmlNode componentNode in componentNodes)
                {   
                    var componentType = ChildNodeText(componentNode, "type");
                    var value = ChildNodeText(componentNode, "long_name");
                    switch (componentType)
                    {
                        case "street_address": // indicates a precise street address.
                            break;
                        case "route": // indicates a named route (such as "US 101").
                            break;
                        case "intersection": // indicates a major intersection, usually of two major roads.
                            break;
                        case "street_number": // indicates a major intersection, usually of two major roads.
                            break;
                        case "country": // indicates the national political entity, and is typically the highest order type returned by the Geocoder.
                            candidate.Country = value;
                            break;
                        case "administrative_area_level_1": // indicates a first-order civil entity below the country level. Within the United States, these administrative levels are states. Not all nations exhibit these administrative levels.
                            candidate.State = value;
                            break;
                        case "administrative_area_level_2": // indicates a second-order civil entity below the country level. Within the United States, these administrative levels are counties. Not all nations exhibit these administrative levels.
                            break;
                        case "administrative_area_level_3": // indicates a third-order civil entity below the country level. This type indicates a minor civil division. Not all nations exhibit these administrative levels.
                            break;
                        case "colloquial_area": // indicates a commonly-used alternative name for the entity.
                            break;
                        case "locality": // indicates an incorporated city or town political entity.
                            candidate.City = value;
                            break;
                        case "sublocality": // indicates a first-order civil entity below a locality. For some locations may receive one of the additional types: sublocality_level_1 through to sublocality_level_5. Each sublocality level is a civil entity. Larger numbers indicate a smaller geographic area.
                            break;
                        case "neighborhood": // indicates a named neighborhood
                            break;
                        case "premise": // indicates a named location, usually a building or collection of buildings with a common name
                            break;
                        case "subpremise": // indicates a first-order entity below a named location, usually a singular building within a collection of buildings with a common name
                            break;
                        case "postal_code": // indicates a postal code as used to address postal mail within the country.
                            candidate.PostalCode = value;
                            break;
                        case "natural_feature": // indicates a prominent natural feature.
                            break;
                        case "airport": // indicates an airport.
                            break;
                        case "park": // indicates a named park.
                            break;
                        case "point_of_interest": // indicates a named point of interest. Typically, these "POI"s are prominent local entities that don't easily fit in another category such as "Empire State Building" or "Statue of Liberty."
                            break;
                    }
                }
            }

            candidate.MatchScore = ScoreCandidate(XMLCandidate);

            return candidate;
        }
    }
}