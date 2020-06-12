using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AxRetailSO.Models
{
    public class SalesPool
    {
        [JsonProperty(PropertyName = "No")]
        public int No { get; set; }

        [JsonProperty(PropertyName = "PoolId")]
        public string PoolId { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }
    }
}