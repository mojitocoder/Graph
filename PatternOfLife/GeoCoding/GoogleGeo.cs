using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PatternOfLife.GeoCoding
{
    public class GoogleGeo
    {
        private string apiKey;

        public GoogleGeo(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public async Task<GoogleGeoResult> ReverseGeoCode(double latitude, double longitude)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://maps.googleapis.com");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var url = $"maps/api/geocode/json?latlng={latitude},{longitude}&key={this.apiKey}";
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var textResult = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GoogleGeoResult>(textResult);
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        public static PostCode GetPostCode(GoogleGeoResult result)
        {
            if (result == null || result.results.Count == 0) return null;

            var addressComponents = result.results.First().address_components;
            if (addressComponents.Count(foo => foo.types.Contains("postal_code")) > 0)
            {
                var postcode = addressComponents.First(foo => foo.types.Contains("postal_code")).short_name;

                string full = postcode.Length > 3 ? postcode : "";
                string init = postcode.Length > 3 ? postcode.Split(' ')[0] : postcode;

                return new PostCode
                {
                    Full = full,
                    Init = init
                };
            }
            else return new PostCode();
        }

        public static string GetFormattedAddress(GoogleGeoResult result)
        {
            if (result == null || result.results.Count == 0) return null;
            else return result.results.First().formatted_address;
        }
    }
}
