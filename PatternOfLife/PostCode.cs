using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternOfLife
{
    public class PostCode
    {
        [JsonProperty(PropertyName = "init")]
        public string Init { get; set; }

        [JsonProperty(PropertyName = "full")]
        public string Full { get; set; }
    }
}
