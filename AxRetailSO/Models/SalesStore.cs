using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AxRetailSO.Models
{
    public class SalesStore
    {
        [JsonProperty(PropertyName = "StoreId")]
        public string StoreId { get; set; }

        [JsonProperty(PropertyName = "StoreName")]
        public string StoreName { get; set; }

        [JsonProperty(PropertyName = "UserAccount")]
        public string UserAccount { get; set; }


    }
}