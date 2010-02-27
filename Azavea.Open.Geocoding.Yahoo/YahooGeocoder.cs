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
using System.Web;
using System.Collections.Generic;
using System.Xml;
using Azavea.Open.Common;
using Azavea.Open.Geocoding.Util;
using Azavea.Open.Reprojection;
using GeoAPI.CoordinateSystems;

namespace Azavea.Open.Geocoding.Yahoo
{
    /// <summary>
    /// Geocodes against Yahoo's geocoder.
    /// </summary>
    public class YahooGeocoder : GeocoderSource
    {
        private readonly string _appId;

        /// <summary>
        /// Geocodes against Yahoo's geocoder.
        /// </summary>
        /// <param name="config">The config file we are using.</param>
        /// <param name="component">The config section we are using to look for RequestProcessors and ResponseProcessors.</param>
        public YahooGeocoder(Config config, string component)
            : base(config, component)
        {
            _appId = config.GetParameter(component, "YahooAppID");
        }

        /// <summary>
        /// This is where actual source-specific geocoding happens.  It is called internal because
        /// clients use <code>Geocode</code> which sandwiches this call between pre- and post-processing.
        /// </summary>
        /// <param name="request">The request to be geocoded.</param>
        /// <returns>A <code>GeocodeResponse</code> describing the results of the geocode.</returns>
        protected override GeocodeResponse InternalGeocode(GeocodeRequest request)
        {
            const string baseYahooGeocodeURL = @"http://api.local.yahoo.com/MapsService/V1/geocode";
            string queryString;
            if (!String.IsNullOrEmpty(request.TextString))
            {
                queryString = "?location="+HttpUtility.UrlEncode(request.TextString);
            }
            else
            {
                queryString = "?street="+HttpUtility.UrlEncode(request.Address) + 
                              "&city=" + request.City + 
                              "&state=" + request.State +
                              "&zip=" + request.PostalCode;
            }

            string yahooGeocodeURL = baseYahooGeocodeURL + 
                                     queryString + 
                                     "&output=xml&appid=" + _appId;

            string response = InternetSourceUtil.GetRawHTMLFromURL(yahooGeocodeURL);
            IList<GeocodeCandidate> responseList = XMLList2GeocodeCandidates(response);
            return (new GeocodeResponse(responseList, this));
        }

        /// <summary>
        /// Yahoo always uses WGS84.
        /// </summary>
        public override ICoordinateSystem CoordinateSystem
        {
            get { return Reprojector.WGS84; }
        }

        private static IList<GeocodeCandidate> XMLList2GeocodeCandidates(string xmlList)
        {
            IList<GeocodeCandidate> candidates = new List<GeocodeCandidate>();

            //Namespaces messing up our Xqueries.
            string ns = " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";
            xmlList = xmlList.Remove(xmlList.IndexOf(ns), ns.Length);
            ns = " xmlns=\"urn:yahoo:maps\"";
            xmlList = xmlList.Remove(xmlList.IndexOf(ns), ns.Length);
            ns = " xsi:schemaLocation=\"urn:yahoo:maps http://api.local.yahoo.com/MapsService/V1/GeocodeResponse.xsd\"";
            xmlList = xmlList.Remove(xmlList.IndexOf(ns), ns.Length);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlList);
            XmlNodeList hits = doc.SelectNodes("//ResultSet/Result");

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

            XmlElement resultNode = (XmlElement)XMLCandidate;

            string precision = resultNode.GetAttribute("precision");
            if (precision != null)
            {
                switch(precision)
                {
                    case "address": curCandidate.MatchScore =0; break;
                    case "street": curCandidate.MatchScore =1; break;
                    case "zip+4": curCandidate.MatchScore =2; break;
                    case "zip+2": curCandidate.MatchScore=3; break;
                    case "zip": curCandidate.MatchScore=4; break;
                    case "city": curCandidate.MatchScore=5; break;
                    case "state": curCandidate.MatchScore=6; break;
                    default: curCandidate.MatchScore=7;break;
                }
            }
            else
            {
                curCandidate.MatchScore=8;
            }

            //Address
            XmlElement addressNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::Address");
            if (addressNode != null) curCandidate.Address = addressNode.InnerText;

            //City
            XmlElement cityNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::City");
            if (cityNode != null) curCandidate.City = cityNode.InnerText;

            //State
            XmlElement stateNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::State");
            if (stateNode != null) curCandidate.State = stateNode.InnerText;

            //PostalCode
            XmlElement postalNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::Zip");
            if (postalNode != null) curCandidate.PostalCode = postalNode.InnerText;

            //Latitude
            XmlElement latitudeNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::Latitude");
            if (latitudeNode != null) curCandidate.Latitude = Convert.ToDouble(latitudeNode.InnerText);

            //Longitude
            XmlElement longitudeNode = (XmlElement)XMLCandidate.SelectSingleNode("descendant::Longitude");
            if (longitudeNode != null) curCandidate.Longitude = Convert.ToDouble(longitudeNode.InnerText);

            //Standardized address - constructed from other fields.
            if (curCandidate.Address.Length > 0) curCandidate.StandardizedAddress = curCandidate.Address + ", ";
            if (curCandidate.City.Length > 0) curCandidate.StandardizedAddress += curCandidate.City + ", ";
            if (curCandidate.State.Length > 0) curCandidate.StandardizedAddress += curCandidate.State + ", ";
            if (curCandidate.PostalCode.Length > 0) curCandidate.StandardizedAddress += curCandidate.PostalCode + ", ";
            if (curCandidate.StandardizedAddress.Length > 0)
            {
                curCandidate.StandardizedAddress = curCandidate.StandardizedAddress.Remove(curCandidate.StandardizedAddress.Length -2,2);
            }
            return curCandidate;
        }
    }
}