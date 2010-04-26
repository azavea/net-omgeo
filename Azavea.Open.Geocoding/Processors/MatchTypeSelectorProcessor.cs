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
using Azavea.Open.Common;

namespace Azavea.Open.Geocoding.Processors
{
    /// <summary>
    /// This process examines a list of applicable MatchTypes.  If the Candidate's MatchType is
    /// found in that list, the GeocodeResponse is left in the Candidates list.  If the MatchType
    /// is not found, the Candidate is removed from the list.  If a Candidate doesn't have a
    /// MatchType, this processor will ignore it.
    /// </summary>
    public class MatchTypeSelectorProcessor : IResponseProcessor
    {
        private readonly List<string> _allowedMatchTypes;

        /// <summary>
        /// Create a new Processor with the given configuration
        /// </summary>
        /// <param name="config">An Azavea.Open Config object</param>
        /// <param name="sectionName">The section with the country list required by this processor</param>
        public MatchTypeSelectorProcessor(Config config, string sectionName)
        {
            string matchList = config.GetParameter(sectionName, "MatchTypes");
            if (matchList != null)
            {
                _allowedMatchTypes = new List<string>(matchList.Split('|'));
            }
            else throw new LoggingException("No MatchType list in this config");
        }


        public void ProcessResponse(GeocodeResponse response)
        {
            if (response.HasCandidates)
            {
                response.Candidates.RemoveAll(IsNotAllowedByMatchType);
            }
        }

        private bool IsNotAllowedByMatchType(GeocodeCandidate candidate)
        {
            string type = candidate.MatchType;
            if (!string.IsNullOrEmpty(type) && !_allowedMatchTypes.Contains(type))
            {
                return true;
            }
            return false;
        }
    }
}
