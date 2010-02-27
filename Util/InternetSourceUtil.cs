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

using System.IO;
using System.Net;
using System.Text;

namespace Azavea.Open.Geocoding.Util
{
    /// <summary>
    /// This class provides some simple utility functions for Internet- or Web-based
    /// geocoders to share code.
    /// </summary>
    public static class InternetSourceUtil
    {
        /// <summary>
        /// Gets HTML  data (or potentially any data) by making a request to a URL.
        /// </summary>
        /// <param name="url">The URL to request.</param>
        /// <returns>The response text.</returns>
        public static string GetRawHTMLFromURL(string url)
        {
            return GetDataFromURL(url, null);
        }

        /// <summary>
        /// Returns Raw Text from a URL. Adds line breaks where needed
        /// </summary>
        /// <param name="url">The URL to request.</param>
        /// <returns>The response text, with a newline after each line.</returns>
        public static string GetRawTextFromURL(string url)
        {
            return GetDataFromURL(url, "\r\n");
        }

        /// <summary>
        /// Gets data by aking a request to a URL, and adds an arbitrary line ending.
        /// </summary>
        /// <param name="url">The URL to request.</param>
        /// <param name="lineEnding">A string that will be attached after each line
        /// ending in the response text.</param>
        /// <returns>The response text, with the optional line ending after each line.</returns>
        private static string GetDataFromURL(string url, string lineEnding)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(url);
            StreamReader sr = new StreamReader(stream);
            string line;
            StringBuilder sb = new StringBuilder();

            while ((line = sr.ReadLine()) != null)
            {
                sb.Append(line + lineEnding);
            }

            return sb.ToString();
        }
    }
}