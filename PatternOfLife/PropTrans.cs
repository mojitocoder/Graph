using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternOfLife
{
    public class PropTrans
    {
        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }

        [JsonProperty(PropertyName = "dateoftransfer")]
        public DateTime DateOfTransfer { get; set; }

        [JsonProperty(PropertyName = "postcode")]
        public string PostCode { get; set; }

        [JsonProperty(PropertyName = "postcodeinit")]
        public string PostCodeInit { get; set; }
        //D Detached
        //S Semi-Detached
        //T   Terraced
        //F   Flats
        //O   Other

        [JsonProperty(PropertyName = "proptype")]
        public char PropertyType { get; set; }

        [JsonProperty(PropertyName = "new")]
        public bool NewBuilding { get; set; }

        [JsonProperty(PropertyName = "freehold")]
        public bool Freehold { get; set; }

        [JsonProperty(PropertyName = "paon")]
        public string PAON { get; set; }

        [JsonProperty(PropertyName = "saon")]
        public string SAON { get; set; }

        [JsonProperty(PropertyName = "street")]
        public string Street { get; set; }

        [JsonProperty(PropertyName = "locality")]
        public string Locality { get; set; }

        [JsonProperty(PropertyName = "town")]
        public string Town { get; set; }

        [JsonProperty(PropertyName = "district")]
        public string District { get; set; }

        [JsonProperty(PropertyName = "county")]
        public string County { get; set; }

        [JsonProperty(PropertyName = "stdprice")]
        public bool StandardPrice { get; set; }

        [JsonProperty(PropertyName = "recstatus")]
        public char RecordStatus { get; set; }
    }
}
