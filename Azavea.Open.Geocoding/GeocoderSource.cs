extern alias newGeoAPI;
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
using System.Configuration;
using System.Reflection;
using Azavea.Open.Common;
using Azavea.Open.Geocoding.Processors;
using newGeoAPI::GeoAPI.CoordinateSystems;
using log4net;

namespace Azavea.Open.Geocoding
{
    /// <summary>
    /// Any source usable by the geocoder must implement this class.
    /// NOTE: All classes implementing this must have a constructor signature like this:
    /// public MyClass(Config config, string sectionName) {...}.
    /// You do not have to USE the config, but you have to accept it, because
    /// GeocoderSources are constructed with reflection.
    /// </summary>
    public abstract class GeocoderSource
    {
        /// <summary>
        /// log4net logger for logging any appropriate messages.
        /// </summary>
        protected ILog _log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);
        private readonly Dictionary<Type, IList<IProcessor>> _processors = new Dictionary<Type, IList<IProcessor>>();

        /// <summary>
        /// This constructor does the default stuff of getting the pre- and post-processors
        /// from the config section.
        /// </summary>
        /// <param name="config">The config file we are using.</param>
        /// <param name="sectionName">The config section we are using to look for RequestProcessors and ResponseProcessors.</param>
        protected GeocoderSource(Config config, string sectionName)
        {
            _processors[typeof(IRequestProcessor)] = new List<IProcessor>();
            _processors[typeof(IResponseProcessor)] = new List<IProcessor>();
            GetProcessors(config, sectionName);
        }

        /// <summary>
        /// This handles running through the configuration and getting the request
        /// and response processors.
        /// </summary>
        /// <param name="config">The config file we are using.</param>
        /// <param name="sectionName">The config section we are using to look for RequestProcessors and ResponseProcessors.</param>
        private void GetProcessors(Config config, string sectionName)
        {
            if (config.ComponentExists(sectionName))
            {
                IList<KeyValuePair<string, string>> kvps = config.GetParametersAsList(sectionName);
                foreach (KeyValuePair<string, string> kvp in kvps)
                {
                    string typeName = kvp.Value;
                    Type processorType = Type.GetType(typeName);
                    if (processorType != null && typeof(IProcessor).IsAssignableFrom(processorType))
                    {
                        ConstructorInfo ci = processorType.GetConstructor(new [] {typeof (Config), typeof (string)});
                        IProcessor p;
                        if (ci != null)
                        {
                            p = (IProcessor) ci.Invoke(new object[] {config, kvp.Key});
                        }
                        else
                        {
                            ci = processorType.GetConstructor(new Type[] {});
                            if (ci == null)
                            {
                                throw new ConfigurationErrorsException("Processor '" + typeName +
                                                                       "' was specified, but we were unable to get constructor info.");
                            }
                            p = (IProcessor) ci.Invoke(new object[] {});
                        }

                        // At this point we have a processor object.  Add it to the dictionary.
                        if (p as IRequestProcessor != null)
                        {
                            _processors[typeof (IRequestProcessor)].Add(p);
                        }
                        if (p as IResponseProcessor != null)
                        {
                            _processors[typeof (IResponseProcessor)].Add(p);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Geocodes the given input and returns a response.
        /// </summary>
        /// <param name="geocodeRequest">The request to geocode.</param>
        /// <returns>The response to the request, will not be null even if there are no
        ///          matches; rather it will have an empty list of candidates.</returns>
        public GeocodeResponse Geocode(GeocodeRequest geocodeRequest)
        {
            GeocodeRequest processedRequest = ProcessRequest(geocodeRequest);
			GeocodeResponse response = InternalGeocode(processedRequest);
            ProcessResponse(response);
            return response;
        }

        /// <summary>
        /// Geocodes more than one input at a time.
        /// </summary>
        /// <param name="gReqs">A group of requests to geocode.</param>
        /// <returns>A group of results, one for each request,
        ///          in the same order as the requests.</returns>
        public IList<GeocodeResponse> Geocode(IEnumerable<GeocodeRequest> gReqs)
        {
            List<GeocodeRequest> processedRequests = new List<GeocodeRequest>();

            // pre-process
            foreach (GeocodeRequest gReq in gReqs)
            {
                GeocodeRequest processedRequest = ProcessRequest(gReq);
                processedRequests.Add(processedRequest);
            }

            // geocode
            IList<GeocodeResponse> responses = InternalBatchGeocode(processedRequests);

            // post-process
            foreach (GeocodeResponse resp in responses)
            {
                ProcessResponse(resp);
            }

            return responses;
        }

        /// <summary>
        /// Takes a geocoding request, and runs it through all of the RequestProcessors.
        /// </summary>
        /// <param name="geocodeRequest"></param>
        private GeocodeRequest ProcessRequest(GeocodeRequest geocodeRequest)
        {
        	GeocodeRequest modifiedRequest = new GeocodeRequest(geocodeRequest);
            foreach (IRequestProcessor rp in _processors[typeof(IRequestProcessor)])
            {
                modifiedRequest = rp.ProcessRequest(modifiedRequest);
            }

        	return modifiedRequest;
        }

        /// <summary>
        /// Takes a geocoding response, and runs it through all of the ResponseProcessors.
        /// </summary>
        /// <param name="geocodeResponse"></param>
        private void ProcessResponse(GeocodeResponse geocodeResponse)
        {
            foreach (IResponseProcessor rp in _processors[typeof(IResponseProcessor)])
            {
                rp.ProcessResponse(geocodeResponse);
            }
        }

        /// <summary>
        /// This is where actual source-specific geocoding happens.  It is called internal because
        /// clients use <code>Geocode</code> which sandwiches this call between pre- and post-processing.
        /// </summary>
        /// <param name="request">The request to be geocoded.</param>
        /// <returns>A <code>GeocodeResponse</code> describing the results of the geocode.</returns>
        protected abstract GeocodeResponse InternalGeocode(GeocodeRequest request);

        /// <summary>
        /// This virtual method is a non-batch implementation of InternalBatchGeocode.  If your
        /// 'GeocoderSource' object supports it, please override this function with a batch geocode
        /// implementation.
        /// </summary>
        /// <param name="gReqs">A generic list of GeocodeRequests</param>
        /// <returns>A generic list of GeocodeResponses</returns>
        protected virtual IList<GeocodeResponse> InternalBatchGeocode(IList<GeocodeRequest> gReqs)
        {
            IList<GeocodeResponse> responses = new List<GeocodeResponse>();

            // TODO: This should check for IGeocoderBatchSources and allow those
            // to geocode as a batch, rather than always using the single-request
            // signature.
            foreach (GeocodeRequest gReq in gReqs)
            {
                responses.Add(Geocode(gReq));
            }

            return responses;
        }

        /// <summary>
        /// Returns the coordinate this geocoder used.
        /// </summary>
        public abstract ICoordinateSystem CoordinateSystem { get; }

		/// <summary>
		/// A list of request processors configured on this geocoder source.
		/// </summary>
    	public IList<IProcessor> RequestProcessors
    	{
			get 
			{ 
				return _processors[ typeof ( IRequestProcessor ) ];
			}
    	}

		/// <summary>
		/// A list of response processors configured on this geocoder source.
		/// </summary>
    	public IList<IProcessor> ResponseProcessors
    	{
			get
			{
				return _processors[ typeof ( IResponseProcessor ) ];
			}
    	}
    }
}
