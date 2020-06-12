using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AxRetailSO.Models
{
    public class User
    {
        [JsonProperty(PropertyName = "Username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "UserType")]
        public string UserType { get; set; }

        [JsonProperty(PropertyName = "Password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "Page")]
        public string Page { get; set; }

        [JsonProperty(PropertyName = "PageSize")]
        public string PageSize { get; set; }

        [JsonProperty(PropertyName = "TextSearch")]
        public string TextSearch { get; set; }
    }
}