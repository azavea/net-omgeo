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
using System.Text.RegularExpressions;

namespace Azavea.Open.Geocoding.Tests
{
    /// <exclude/>
    public static class TestUtils
    {
        /// <exclude/>
        public static void OutputGeocodeResponses(GeocodeResponse gr)
        {
            Console.WriteLine("**************Begin Response*************");

            if (gr.HasCandidates)
            {
                foreach (GeocodeCandidate gc in gr.Candidates)
                {
                    Console.WriteLine("Standardized Address: " + gc.StandardizedAddress);
                    Console.WriteLine("Address: " + gc.Address);
                    Console.WriteLine("City: " + gc.City);
                    Console.WriteLine("State: " + gc.State);
                    Console.WriteLine("PostalCode: " + gc.PostalCode);
                    Console.WriteLine("Match Score: " + gc.MatchScore);
                    Console.WriteLine("X,Y: " + gc.Longitude + ", " + gc.Latitude);
                    Console.WriteLine("Additional Data: " + gc.RawData);


                    Regex parsableAddressParts = new Regex(@"^((?<address>\d{1,}(\D|\s1/2)?(?:\s?-\s?\d{1,})*\s(?>[^#,]+)(?>#\s*.+)?),)?\s*((?<zip>\d{5}(-\d{4})?)\s*,)?\s*(?<city>[^,]+)\s*,(\s*(Town Of)\s*,)?\s*(?<state>[A-Z]{2})(\s+(?<zip2>\d{5}(-\d{4})?))?", RegexOptions.IgnoreCase);
                    if (parsableAddressParts.IsMatch(gc.StandardizedAddress))
                    {
                        Match m = parsableAddressParts.Match(gc.StandardizedAddress);
                        GroupCollection groups = m.Groups;
                        Console.WriteLine("\t" + groups["address"].Value.Trim());
                        Console.WriteLine("\t" + groups["city"].Value.Trim());
                        Console.WriteLine("\t" + groups["state"].Value.Trim());
                        Console.WriteLine("\t" + groups["zip"].Value.Trim());
                    }
                    Console.WriteLine("**********");
                    Console.WriteLine("");

                }
            }
            else
            {
                Console.WriteLine("No Responses Received");
            }

            Console.WriteLine("**************End Response*************");
        }
    }
}
