using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AxRetailSO.Models
{
    public class CalendarRange
    {
        [JsonProperty(PropertyName = "Range")]
        public string range { get; set; }
    }
}