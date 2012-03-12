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
using System.Text;
using Azavea.Open.Common;

namespace Azavea.Open.Geocoding.Processors
{
    class ScoreNormalizer : IResponseProcessor
    {
        /// <summary>
        /// A dictionary of MatchType names with an integer representing how many points should be added to the 
        /// Candidates from that MatchType.  Unless you want negative scores, don't use a negative number for these.
        /// </summary>
        private Dictionary<string, int> _scoreModifiers;

        ///<exclude />
        private double _maxNormalizedScore;

        /// <summary>
        /// Create a new ScoreNormalizer with the given config.  Specify a "Modifiers" parameter in your config
        /// to weight the scores.  The config is a string in the format "LocatorName:Score|LocatorName:Score".
        /// See the tests for an example
        /// </summary>
        /// <param name="config">An Azavea Config object</param>
        /// <param name="component">The component with the configuration for this normalizer</param>
        public ScoreNormalizer(Config config, string component)
        {
            _scoreModifiers = new Dictionary<string, int>();
            string modifierConfig = config.GetParameter(component, "Modifiers");
            if (!string.IsNullOrEmpty(modifierConfig))
            {
                string[] configs = modifierConfig.Split('|');
                foreach (string s in configs)
                {
                    string[] values = s.Split(':');
                    if (values.Length == 2)
                    {
                        int score;
                        if (int.TryParse(values[1], out score))
                        {
                            _scoreModifiers.Add(values[0], score);
                        }
                    }
                }
            }
            if (_scoreModifiers.Count == 0)
            {
                throw new LoggingException("No modifier list in this config");
            }
        }

        public void ProcessResponse(GeocodeResponse response)
        {
            if (_scoreModifiers.Count > 0 && response.HasCandidates)
            {
                DistributeScores(response.Candidates);
                NormalizeCandidates(response.Candidates);
            }
        }

        /// <summary>
        /// Distribute the Scores according to the Modifiers listed in the configuration file
        /// </summary>
        /// <param name="candidates">A list of GeocodeCandidates with MatchScores to distribute</param>
        private void  DistributeScores (List<GeocodeCandidate> candidates)
        {
            foreach (GeocodeCandidate candidate in candidates)
            {
                if (_scoreModifiers.ContainsKey(candidate.MatchType))
                {
                    candidate.MatchScore += _scoreModifiers[candidate.MatchType];
                }
                _maxNormalizedScore = Math.Max(candidate.MatchScore, _maxNormalizedScore);
            }
        }

        /// <summary>
        /// Take the MatchScores of the given candidates and put them on a scale between 1 and 100, taking
        /// into account the highest possible score due to modifiers.
        /// </summary>
        /// <param name="candidates">A list of GeocodeCandidates with MatchScores to normalize</param>
        private void NormalizeCandidates(List<GeocodeCandidate> candidates)
        {

            double weight = Math.Ceiling(_maxNormalizedScore/ 100);

            foreach (GeocodeCandidate candidate in candidates)
            {
                candidate.MatchScore /= weight;
            }
        }
    }
}
