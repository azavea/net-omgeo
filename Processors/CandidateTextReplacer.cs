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
using System.Reflection;
using System.Text.RegularExpressions;
using Azavea.Open.Common;

namespace Azavea.Open.Geocoding.Processors
{
    /// <summary>
    /// This response processor iterates through the candidates in a response
    /// and for each one finds a given field (ReplaceField), and replaces any
    /// occurences of a certain string (Find) with another string (ReplaceWith).
    /// </summary>
    public class CandidateTextReplacer : IResponseProcessor
    {
        private readonly PropertyInfo _replaceField;
        private readonly string _matchRegex;
        private readonly string _replaceWith;
        private readonly PropertyInfo[] _sourceFields;

        /// <summary>
        /// Get the config for the field and text to be replaced.
        /// </summary>
        /// <param name="config">The config file to use.</param>
        /// <param name="component">The component to use.  This should have three
        /// parameters: ReplaceField, Find, and ReplaceWith.</param>
        public CandidateTextReplacer(Config config, string component)
        {
            _replaceField = typeof (GeocodeCandidate).GetProperty(config.GetParameter(component, "ReplaceField"));
            _matchRegex = config.GetParameter(component, "Find");
            _replaceWith = config.GetParameter(component, "ReplaceWith");
            MatchCollection matches = Regex.Matches(_replaceWith, "{.*?}");
            _sourceFields = new PropertyInfo[matches.Count];
            for (int i = 0; i < _sourceFields.Length; i++)
            {
                string sourceFieldName = matches[i].Value.TrimStart('{').TrimEnd('}');
                _sourceFields[i] = typeof (GeocodeCandidate).GetProperty(sourceFieldName);
            }
        }

        /// <summary>
        /// Implementations of this method will be used to process a geocode response, to
        /// weed out low-scoring candidates, for example.
        /// </summary>
        /// <param name="response">The response object to be modified.</param>
        public void ProcessResponse(GeocodeResponse response)
        {
            foreach (GeocodeCandidate candidate in response.Candidates)
            {
                string fieldVal = (string) _replaceField.GetValue(candidate, null) ?? "";
                string thisReplacer = _replaceWith;
                string[] sourceFieldData = new string[_sourceFields.Length];
                for (int i = 0; i < _sourceFields.Length; i++)
                {
                    sourceFieldData[i] = (string) _sourceFields[i].GetValue(candidate, null);
                }
                int l = 0;
                for (int i = 0; l >= 0; i++ )
                {
                    l = thisReplacer.IndexOf('{', l);
                    if (l == -1)
                    {
                        break;
                    }
                    thisReplacer = thisReplacer.Remove(l + 1, thisReplacer.IndexOf('}', l) - l - 1);
                    thisReplacer = thisReplacer.Insert(l + 1, i.ToString());
                    l++;
                }
                thisReplacer = String.Format(thisReplacer, sourceFieldData);
                fieldVal = Regex.Replace(fieldVal, _matchRegex, thisReplacer);
                _replaceField.SetValue(candidate, fieldVal, null);
            }
        }
    }
}
