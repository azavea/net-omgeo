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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Avencia.Open.Common;
using Avencia.Open.Geocoding.Processors;
using Avencia.Open.Reprojection;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;

namespace Avencia.Open.Geocoding
{
    /// <summary>
    /// The main geocoding class.  This will inspect configuration, and use that to define the
    /// list of geocoding sources it will use.
    /// </summary>
    public class Geocoder : GeocoderSource
    {
        /// <summary>
        /// List of geocoder sources to use, in order.
        /// </summary>
        private readonly List<GeocoderSource> _geocoderSources = new List<GeocoderSource>();

        /// <summary>
        /// Constructs the geocoder, reading the list of sources from the default
        /// section ("GeocodingSources") of the default config file ("Avencia.Open.Geocoding").
        /// </summary>
        public Geocoder()
            : this("Avencia.Open.Geocoding", "GeocoderSources") { }

        /// <summary>
        /// Constructs the geocoder, reading the list of sources from the default
        /// section ("GeocodingSources") of the specified config file.
        /// </summary>
        /// <param name="configName">Name of the config with the geocoder configuration.</param>
        public Geocoder(string configName)
            : this(configName, "GeocoderSources") { }

        /// <summary>
        /// Constructs the geocoder, reading the list of sources from the specified
        /// section of the specified config file.
        /// </summary>
        /// <param name="configName">Name of the config with the geocoder configuration.</param>
        /// <param name="sectionName">Section of the config that the geocoding sources are in.</param>
        public Geocoder(string configName, string sectionName)
            : this(Config.GetConfig(configName), sectionName) { }

        /// <summary>
        /// Constructs the geocoder, reading the list of sources from the default
        /// section ("GeocodingSources") of the given config.
        /// </summary>
        /// <param name="config">Config object with the geocoder configuration.</param>
        public Geocoder(Config config)
            : this(config, "GeocoderSources") { }

        /// <summary>
        /// Constructs the geocoder, reading the list of sources from the specified
        /// section of the given config.
        /// </summary>
        /// <param name="config">Config object with the geocoder configuration.</param>
        /// <param name="sectionName">Section of the config that the geocoding sources are in.</param>
        public Geocoder(Config config, string sectionName) : base(config, sectionName)
        {
            IList<KeyValuePair<string, string>> sourceLines = config.GetParametersAsList(sectionName);
            foreach (KeyValuePair<string,string> sourceLine in sourceLines)
            {
                try
                {
                    // Key is the config section name for that geocoder source, 
                    string sourceConfigSection = sourceLine.Key;
                    // value is the class name.
                    string typeName = sourceLine.Value;
                    Type geocoderSourceType = Type.GetType(typeName);
                    if (geocoderSourceType == null)
                    {
                        throw new ConfigurationErrorsException("GeocoderSourceClass '" + typeName +
                                                               "' was specified, but we were unable to get type info.  Are you missing a DLL?");
                    }

                    Type[] constructorParamTypes = new[] {typeof (Config), typeof (string)};
                    ConstructorInfo constr = geocoderSourceType.GetConstructor(constructorParamTypes);
                    if (constr == null)
                    {
                        throw new ConfigurationErrorsException("GeocoderSourceClass '" + typeName +
                                                               "' was specified, but we were unable to get constructor info.");
                    }

                    GeocoderSource source = (GeocoderSource) constr.Invoke(new object[] {config, sourceConfigSection});
                    _geocoderSources.Add(source);
                }
                catch (Exception e)
                {
                    throw new LoggingException("Unable to load geocoder source '" +
                        sourceLine.Key + "', '" + sourceLine.Value + "'.", e);
                }
            }
            if (_geocoderSources.Count == 0)
            {
                throw new LoggingException("No geocoding sources specified in section " + sectionName +
                    ", unable to construct geocoder.");
            }
        }

        /// <summary>
        /// Constructs the geocoder with the specified list of sources.  The sources
        /// will be used in the order provided.
        /// </summary>
        /// <param name="sources">Ordered group of sources to use for geocoding.</param>
        public Geocoder(IEnumerable<GeocoderSource> sources) : base(null, null)
        {
            _geocoderSources.AddRange(sources);
            if (_geocoderSources.Count == 0)
            {
                throw new LoggingException("Cannot construct a geocoder with no sources.");
            }
        }

        private ICoordinateSystem _coordinateSystem;

        /// <summary>
        /// Returns the coordinate system this geocoder used.
        /// </summary>
        public override ICoordinateSystem CoordinateSystem
        {
            get { return _coordinateSystem; }
        }
		
        /// <summary>
        /// Returns the number of geocoder sources configured.
        /// </summary>
    	public int SourceCount
    	{
			get { return _geocoderSources.Count; }
    	}

        /// <summary>
        /// Geocodes the given input and returns a response.
        /// </summary>
        /// <param name="request">The request to geocode.</param>
        /// <returns>The response to the request, will not be null even if there are no
        ///          matches.</returns>
        protected override GeocodeResponse InternalGeocode(GeocodeRequest request)
        {
            GeocodeResponse gRes = null;
            foreach (GeocoderSource gs in _geocoderSources)
            {
                gRes = gs.Geocode(request);
                if (gRes.Candidates.Count > 0)
                {
                    // If a specific coordinate system was requested, 
                    // reproject from the goeocoder coordinate system to the
                    // requested coordinate system.  We also need to identify
                    // what CS we are returning in as well.
                    _coordinateSystem = gs.CoordinateSystem;
                    if (request.CoordinateSystem != null)
                    {
                        foreach (GeocodeCandidate candidate in gRes.Candidates)
                        {
                            IPoint p = Reprojector.Reproject(gs.CoordinateSystem, request.CoordinateSystem,
                                                             candidate.Longitude,
                                                             candidate.Latitude);

                            candidate.Longitude = p.X;
                            candidate.Latitude = p.Y;
                        }
                    }
                    break;
                }
            }
            if (gRes == null)
            {
                throw new LoggingException(
                    "Internal state error, should never have a null object at this point. Num sources: " +
                    _geocoderSources.Count);
            }
            return gRes;
        }

        /// <summary>
        /// This figures out which of the list of geocoding sources supports batch geocoding
        /// internally.  If they do, it uses the batch geocoding function; if not, it just
        /// iterates through the requests and geocodes each one.
        /// </summary>
        /// <param name="processedRequests">The requests, already pre-processed.</param>
        /// <returns>A list of responses, which then will need to be post-processed.</returns>
        protected override IList<GeocodeResponse> InternalBatchGeocode(IList<GeocodeRequest> processedRequests)
        {
            IList<GeocodeResponse> responses = null;
            foreach (GeocoderSource gs in _geocoderSources)
            {
                if (typeof(IGeocoderBatchSource).IsAssignableFrom(gs.GetType()))
                {
                    responses = ((IGeocoderBatchSource)gs).InternalBatchGeocode(processedRequests);
                }
                else
                {
                    responses = new List<GeocodeResponse>();
                    foreach (GeocodeRequest req in processedRequests)
                    {
                        responses.Add(InternalGeocode(req));
                    }
                }

                if (responses != null)
                {
                    ReprojectResponses(gs, processedRequests, responses);
                }

            }
            if (responses == null)
            {
                throw new LoggingException(
                    "Internal state error, should never have a null object at this point. Num sources: " +
                    _geocoderSources.Count);
            }
            return responses;
        }

        /// <summary>
        /// Given a list of requests, and a matching list of responses, 
        /// this function reprojects the responses to match the request coordinate system
        /// </summary>
        /// <param name="gs">The GeocoderSource that was used</param>
        /// <param name="processedRequests">The initial list of requests</param>
        /// <param name="responses">The corresponding list of responses</param>
        protected void ReprojectResponses(GeocoderSource gs, IList<GeocodeRequest> processedRequests, IList<GeocodeResponse> responses)
        {
            // If a specific coordinate system was requested, 
            // reproject from the goeocoder coordinate system to the
            // requested coordinate system.  We also need to identify
            // what CS we are returning in as well.
            if (processedRequests.Count != responses.Count)
                return;

            for (int i = 0, max = processedRequests.Count; i < max; i++)
            {
                GeocodeRequest request = processedRequests[i];
                GeocodeResponse response = responses[i];

                if (response.Candidates.Count == 0)
                    continue;

                _coordinateSystem = gs.CoordinateSystem;
                if (request.CoordinateSystem != null)
                {
                    foreach (GeocodeCandidate candidate in response.Candidates)
                    {
                        IPoint p = Reprojector.Reproject(gs.CoordinateSystem, request.CoordinateSystem,
                                                         candidate.Longitude,
                                                         candidate.Latitude);
                        candidate.Longitude = p.X;
                        candidate.Latitude = p.Y;
                    }
                }
            }
        }


		/// <summary>
		/// Get a list of request processors associated with a geocoder source.
		/// </summary>
		/// <param name="index">The geocoder source to get processors from.</param>
		/// <returns>A list of processors, or an empty list if the index is out of bounds.</returns>
		public IList<IProcessor> GetRequestProcessors( int index )
		{
			if ( index >= 0 && index < _geocoderSources.Count )
				return _geocoderSources[ index ].RequestProcessors;

			return new List<IProcessor>();
		}

		/// <summary>
		/// Get a list of response processors associated with a geocoder source.
		/// </summary>
		/// <param name="index">The geocoder source to get processors from.</param>
		/// <returns>A list of processors, or an empty list if the index is out of bounds.</returns>
		public IList<IProcessor> GetResponseProcessors( int index )
		{
			if (index >= 0 && index < _geocoderSources.Count)
				return _geocoderSources[index].ResponseProcessors;

			return new List<IProcessor>();
		}
    }
}
