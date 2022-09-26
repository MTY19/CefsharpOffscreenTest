using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.OffScreen.Models
{
    public class Vehicle
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string bodystyle { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool cpo_indicator { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string customer_id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string horizontal_position { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string listing_id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string make { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string model { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int model_year { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string msrp { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string price { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool sponsored { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string stock_type { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string trim { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int vertica_position { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string web_page_type_from { get; set; }
    }
}
