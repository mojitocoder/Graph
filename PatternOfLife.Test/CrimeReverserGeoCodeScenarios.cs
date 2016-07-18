using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PatternOfLife.Test
{
    public class CrimeReverserGeoCodeScenarios
    {
        const string apiKey = "AIzaSyBiEgORpbZqJP0tsn6bQSeQJRtrYgvtHUc"; //test API key

        [Fact]
        public void Start_Successfully()
        {
            var folder = @"Data\Crime";

            var crimeGeo = new CrimeReverserGeoCode(folder, apiKey);
            crimeGeo.Start();
        }
    }
}
