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
using System.Text;
using System.Text.RegularExpressions;
using Azavea.Open.Common;

namespace Azavea.Open.Geocoding.Processors
{
    /// <summary>
    /// This response processor iterates through the candidates in a response
    /// and for each one finds a given field (ReplaceField), and replaces any
    /// occurences of a certain string (Find) with another string (ReplaceWith).
    /// </summary>
    public class SpecialCharacterRemover : IRequestProcessor
    {
        private readonly string _charactersToRemove = @"@$%*^(){}[]<>~`:;\=|?!";

        /// <summary>
        /// Get the config for characters to be removed if the defaults aren't good enough.
        /// </summary>
        /// <param name="config">The config file to use.</param>
        /// <param name="component">The component to use.  Only one parameter is
        /// available and it's optional: CharactersToRemove</param>
        public SpecialCharacterRemover(Config config, string component)
        {
            if (config.ParameterExists(component, "CharactersToRemove"))
            {
                _charactersToRemove = config.GetParameter(component, "CharactersToRemove");
            }
        }

        #region Implementation of IRequestProcessor

        /// <summary>
        /// This prcoess will remove the desired characters from the address 
        /// fields of a GeocodeRequest.
        /// </summary>
        public GeocodeRequest ProcessRequest(GeocodeRequest request)
        {
            GeocodeRequest copy = new GeocodeRequest(request);
            copy.TextString = RemoveSpecialCharacters(copy.TextString);
            copy.Address = RemoveSpecialCharacters(copy.Address);
            copy.City = RemoveSpecialCharacters(copy.City);
            copy.State = RemoveSpecialCharacters(copy.State);
            copy.PostalCode = RemoveSpecialCharacters(copy.PostalCode);
            copy.Country = RemoveSpecialCharacters(copy.Country);

            return copy;
        }

        #endregion

        private string RemoveSpecialCharacters(string text)
        {
            if (text == null) return null;
            StringBuilder sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (_charactersToRemove.IndexOf(c) == -1)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

    }
}
