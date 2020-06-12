using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AxRetailSO.Models
{
    public class ProductSeries
    {
        [JsonProperty(PropertyName = "Model")]
        public string Model { get; set; }

        [JsonProperty(PropertyName = "Series")]
        public string Series { get; set; }

        [JsonProperty(PropertyName = "Pool")]
        public string Pool { get; set; }
    }
}