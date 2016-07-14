using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PatternOfLife.Test
{
    public class CrimeReaderScenarios
    {
        [Fact]
        public void GetCrime_Successful()
        {
            var path = @"Data\2016-04-cumbria-street.csv";
            var crimeReader = new CrimeReader(path);
            var crimes = crimeReader.Read().ToList();

            Assert.Equal(crimes.Count, 4);
            Assert.Equal(crimes.Select(foo => foo.Id), new List<string> { "", "eeca5424b25a29bf4153518ad25cf56430de76027ec095c83a1ed749c97bce80", "1abdbc75a5327c0d1282f124e384d243e7471cedbd9404f17ee781aa3755d21c", "8fa478462de052d6c38cd29404b4d47f14372f8908f2ae0525998b550c9c01e2" });
            Assert.Equal(crimes.Select(foo => foo.Month), new List<int> { 4, 4, 4, 4 });
            Assert.Equal(crimes.Select(foo => foo.Year), new List<int> { 2016, 2016, 2016, 2016 });
            Assert.Equal(crimes.Select(foo => foo.ReportedBy), new List<string> { "Cumbria Constabulary", "Cumbria Constabulary", "Cumbria Constabulary", "Cumbria Constabulary" });
            Assert.Equal(crimes.Select(foo => foo.Longitude), new List<double> { -3.38862, -3.386349, -3.384752, -3.386686 });
            Assert.Equal(crimes.Select(foo => foo.Latitude), new List<double> { 54.868943, 54.869517, 54.871108, 54.868884 });
            Assert.Equal(crimes.Select(foo => foo.Location), new List<string> { "On or near Nightclub", "On or near Esk Street", "On or near Petrol Station", "On or near Wampool Street" });
            Assert.Equal(crimes.Select(foo => foo.LsoaCode), new List<string> { "E01019126", "E01019126", "E01019126", "E01019126" });
            Assert.Equal(crimes.Select(foo => foo.LsoaName), new List<string> { "Allerdale 001A", "Allerdale 001A", "Allerdale 001A", "Allerdale 001A" });
            Assert.Equal(crimes.Select(foo => foo.Type), new List<string> { "Anti-social behaviour", "Burglary", "Other theft", "Shoplifting" });
            Assert.Equal(crimes.Select(foo => foo.OutcomeCategory), new List<string> { "", "Under investigation", "Investigation complete; no suspect identified", "Offender given penalty notice" });
            Assert.Equal(crimes.Select(foo => foo.Context), new List<string> { "", "", "", "" });
        }
    }
}
