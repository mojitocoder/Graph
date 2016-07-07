using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternOfLife
{
    public class GpRecord

    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "grouping")]
        public string NationalGrouping { get; set; }

        [JsonProperty(PropertyName = "health")]
        public string HighLevelHealth { get; set; }

        [JsonProperty(PropertyName = "add1")]
        public string Address1 { get; set; }

        [JsonProperty(PropertyName = "add2")]
        public string Address2 { get; set; }

        [JsonProperty(PropertyName = "add3")]
        public string Address3 { get; set; }

        [JsonProperty(PropertyName = "add4")]
        public string Address4 { get; set; }

        [JsonProperty(PropertyName = "add5")]
        public string Address5 { get; set; }

        [JsonProperty(PropertyName = "postcode")]
        public string PostCode { get; set; }

        [JsonProperty(PropertyName = "postcodeinit")]
        public string PostCodeInit { get; set; }

        [JsonProperty(PropertyName = "opendate")]
        public DateTime? OpenDate { get; set; }

        [JsonProperty(PropertyName = "closedate")]
        public DateTime? CloseDate { get; set; }

        [JsonProperty(PropertyName = "status")]
        public char StatusCode { get; set; }

        [JsonProperty(PropertyName = "type")]
        public char TypeCode { get; set; }

        [JsonProperty(PropertyName = "parent")]
        public string ParentCode { get; set; }

        [JsonProperty(PropertyName = "joindate")]
        public DateTime? JoinParentDate { get; set; }

        [JsonProperty(PropertyName = "leftdate")]
        public DateTime? LeftParentDate { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string ContactNo { get; set; }

        [JsonProperty(PropertyName = "amended")]
        public bool Amended { get; set; }

        [JsonProperty(PropertyName = "careorg")]
        public string CurrentCareOrg { get; set; }
    }
}
