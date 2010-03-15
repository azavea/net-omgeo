using System.Collections.Generic;

using NUnit.Framework;

namespace Azavea.Open.Geocoding.Tests
{
    [TestFixture]
    public class GeocodeCandidateTests
    {
        /// <summary>
        /// Tests to make sure candidate sorting is happening correctly. 
        /// It should be by MatchScore, descending, because most geocoders will return results with the most precision as the highest value.
        /// </summary>
        [Test]
        public void TestCandidateSort()
        {
            GeocodeCandidate lowestCandidate = new GeocodeCandidate { StandardizedAddress = "1234 Street Ave", MatchScore = 0.67d };
            GeocodeCandidate higherCandidate = new GeocodeCandidate { StandardizedAddress = "5678 Avenue St", MatchScore = 0.83d };
            GeocodeCandidate highestCandidate = new GeocodeCandidate { StandardizedAddress = "9876 Boulevard Cyn", MatchScore = 0.99d };

            List<GeocodeCandidate> candidates = new List<GeocodeCandidate>();
            candidates.Add(higherCandidate);
            candidates.Add(lowestCandidate);
            candidates.Add(highestCandidate);

            candidates.Sort();

            Assert.AreEqual(highestCandidate.StandardizedAddress, candidates[0].StandardizedAddress, "Expected different candidate for first value");
            Assert.AreEqual(lowestCandidate.StandardizedAddress, candidates[2].StandardizedAddress, "Expected different candidate for last value");
        }
    }
}
