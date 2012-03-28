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

using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using Azavea.Open.Common;
using Azavea.Open.Geocoding.Util;
using Azavea.Open.Reprojection;
using newGeoAPI::GeoAPI.CoordinateSystems;

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
            const string baseGoogleGeocodeURL = @"http://maps.google.com/maps/geo";

            // For the address, either use the address field or the text string, 
            // depending on which one has a value.  Then, add each following address
            // part, one by one, if it has a value.
            string queryString =
                (!String.IsNullOrEmpty(geocodeRequest.TextString) ? geocodeRequest.TextString : geocodeRequest.Address) +
                (!String.IsNullOrEmpty(geocodeRequest.City) ? (", " + geocodeRequest.City) : "") +
                (!String.IsNullOrEmpty(geocodeRequest.State) ? (", " + geocodeRequest.State) : "") +
                (!String.IsNullOrEmpty(geocodeRequest.PostalCode) ? (" " + geocodeRequest.PostalCode) : "") +
                (!String.IsNullOrEmpty(geocodeRequest.Country) ? (", " + geocodeRequest.Country) : "");

            // add oe=utf-8 here for returning valid utf-8, not iso-8859-1
            string googleGeocodeURL = baseGoogleGeocodeURL +
                                      "?q=" + HttpUtility.UrlEncode(queryString) +
                                      "&output=xml&oe=utf-8&key=" + _authKey;

            string response = InternetSourceUtil.GetRawHTMLFromURL(googleGeocodeURL);
            IList<GeocodeCandidate> responseList = XMLList2GeocodeCandidates(response);
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
            //Namespaces messing up our Xqueries.
            string ns = " xmlns=\"http://earth.google.com/kml/2.0\"";
            xmlList = xmlList.Remove(xmlList.IndexOf(ns), ns.Length);

            ns = " xmlns=\"urn:oasis:names:tc:ciq:xsdschema:xAL:2.0\"";
            while (xmlList.IndexOf(ns) > 0)
            {
                xmlList = xmlList.Remove(xmlList.IndexOf(ns), ns.Length);
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlList);
            XmlNodeList hits = doc.SelectNodes("//kml/Response/Placemark");

            if (hits != null)
            {
                //Parse XML to get geocode candidate data;
                foreach (XmlNode hit in hits)
                {
                    candidates.Add(XMLCandidate2GeocodeCandidate(hit));
                }
            }
            return candidates;
        }

        private static GeocodeCandidate XMLCandidate2GeocodeCandidate(XmlNode XMLCandidate)
        {
            GeocodeCandidate curCandidate = new GeocodeCandidate();
            curCandidate.RawData = XMLCandidate.InnerXml;

            //Test for presence of each tag. If it exists, assign it to appropriate value in Geocode Candidate object.

            //Accuracy - Actually the Gecode *level* of the geocode. Invert.
            //0       Unknown location. 
            //1       Country level accuracy. 
            //2       Region (state, province, prefecture, etc.) level accuracy. 
            //3       Sub-region (county, municipality, etc.) level accuracy. 
            //4       Town (city, village) level accuracy. 
            //5       Post code (zip code) level accuracy. 
            //6       Street level accuracy. 
            //7       Intersection level accuracy. 
            //8       Address level accuracy. 
            //9       Premise (building name, etc) level accuracy
            XmlElement addressDetailsNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::AddressDetails");
            if (addressDetailsNode != null)
            {
                string accuracy = addressDetailsNode.GetAttribute("Accuracy");
                curCandidate.MatchScore = Convert.ToDouble(accuracy);
                switch (accuracy) /* Set our MatchType string from Google's list */
                {
                    case "0":
                        curCandidate.MatchType = "Unknown";
                        break;
                    case "1":
                        curCandidate.MatchType = "Country";
                        break;
                    case "2":
                        curCandidate.MatchType = "Region";
                        break;
                    case "3":
                        curCandidate.MatchType = "Sub-Region";
                        break;
                    case "4":
                        curCandidate.MatchType = "Town";
                        break;
                    case "5":
                        curCandidate.MatchType = "Post code";
                        break;
                    case "6":
                        curCandidate.MatchType = "Street";
                        break;
                    case "7":
                        curCandidate.MatchType = "Intersection";
                        break;
                    case "8":
                        curCandidate.MatchType = "Address";
                        break;
                    case "9":
                        curCandidate.MatchType = "Premise";
                        break;
                }
           }


            //Standardized Address
            XmlElement standardAddressNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::address");
            if (standardAddressNode != null) curCandidate.StandardizedAddress = standardAddressNode.InnerText;

            //Address
            XmlElement addressNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::ThoroughfareName");
            if (addressNode != null) curCandidate.Address = addressNode.InnerText;

            //City
            XmlElement cityNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::LocalityName");
            if (cityNode != null) curCandidate.City = cityNode.InnerText;

            //State
            XmlElement stateNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::AdministrativeAreaName");
            if (stateNode != null) curCandidate.State = stateNode.InnerText;

            //PostalCode
            XmlElement postalNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::PostalCodeNumber");
            if (postalNode != null) curCandidate.PostalCode = postalNode.InnerText;

            //latLong
            XmlElement latLongNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::Point/coordinates");
            if (latLongNode != null)
            {
                string latLong = latLongNode.InnerText;
                string[] location = latLong.Split(',');
                curCandidate.Longitude = Convert.ToDouble(location[0]);
                curCandidate.Latitude = Convert.ToDouble(location[1]);
            }

            return curCandidate;
        }
    }
}