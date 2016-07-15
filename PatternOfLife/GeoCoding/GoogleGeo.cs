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

        public static AddressDetail GetAddressDetail(GoogleGeoResult result)
        {
            if (result == null || result.results.Count == 0) return null;

            var x = result.results.Select(r => new
            {
                FormattedAddress = r.formatted_address,
                PostCode = r.address_components.FirstOrDefault(component => component.types.Contains("postal_code"))
            })
            .Where(foo => foo.PostCode != null)
            .Select(foo => new
            {
                FormattedAddress = foo.FormattedAddress,
                PostCode = foo.PostCode.long_name
            })
            .ToList();

            //empty, no address with postcode at all
            if (x.Count == 0) return new AddressDetail();

            var maxPostCodeLength = x.Select(foo => foo.PostCode.Length).Max();

            //get the first longest postcode
            var address = x.First(foo => foo.PostCode.Length == maxPostCodeLength);

            string full = address.PostCode.Length > 3 ? address.PostCode : "";
            string init = address.PostCode.Length > 3 ? address.PostCode.Split(' ')[0] : address.PostCode;

            return new AddressDetail
            {
                PostCode = full,
                PostCodeInit = init,
                FullAddress = address.FormattedAddress
            };
        }

        public static string GetFormattedAddress(GoogleGeoResult result)
        {
            if (result == null || result.results.Count == 0) return null;
            else return result.results.First().formatted_address;
        }
    }

    public class AddressDetail
    {
        public string PostCode { get; set; }
        public string PostCodeInit { get; set; }
        public string FullAddress { get; set; }
    }
}
