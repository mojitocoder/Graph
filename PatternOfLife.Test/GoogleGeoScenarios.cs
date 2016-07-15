using Newtonsoft.Json;
using PatternOfLife.GeoCoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PatternOfLife.Test
{
    public class GoogleGeoScenarios
    {
        const string apiKey = "AIzaSyBiEgORpbZqJP0tsn6bQSeQJRtrYgvtHUc"; //test API key

        [Fact]
        public async Task ReverseGeoCode_Successful_London()
        {
            var googleGeo = new GoogleGeo(apiKey);
            var result = await googleGeo.ReverseGeoCode(51.497309, -0.172005);

            Assert.Equal(result.status, "OK");
            Assert.Equal(result.results.Count, 11);
            Assert.Equal(result.results.First().formatted_address, "Knightsbridge, London SW7, UK");
        }

        [Fact]
        public async Task ReverseGeoCode_Successful_NewYork()
        {
            var googleGeo = new GoogleGeo(apiKey);
            var result = await googleGeo.ReverseGeoCode(40.714224, -73.961452);

            Assert.Equal(result.status, "OK");
            Assert.Equal(result.results.Count, 11);
            Assert.Equal(result.results.First().formatted_address, "277 Bedford Ave, Brooklyn, NY 11211, USA");
        }

        [Fact]
        public void GetAddressDetail_Correctly()
        {
            var path = @"Data\knightsbridge.json";
            var json = File.ReadAllText(path);
            var geoResult = JsonConvert.DeserializeObject<GoogleGeoResult>(json);

            var address = GoogleGeo.GetAddressDetail(geoResult);
            Assert.Equal(address.PostCodeInit, "SW7");
            Assert.Equal(address.PostCode, "SW7 2PS");
            Assert.Equal(address.FullAddress, "9 Princes Gate Mews, Knightsbridge, London SW7 2PS, UK");
        }
    }
}
