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
        [Fact]
        public void Start_Successfully()
        {
            var folder = @"Data\Crime";

            var crimeGeo = new CrimeReverserGeoCode(folder);
            crimeGeo.Start();
        }
    }
}
