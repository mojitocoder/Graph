using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternOfLife
{
    public class PostCodeInit
    {
        [JsonProperty(PropertyName = "init")]
        public string Init { get; set; }
    }
}
