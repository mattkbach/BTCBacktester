using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCBacktester
{
    public class CandleData
    {
        [JsonProperty("timestamp")]
        public DateTime? timestamp { get; set; }
        [JsonProperty("symbol")]
        public string symbol { get; set; }
        [JsonProperty("open")]
        public double? open { get; set; }
        [JsonProperty("high")]
        public double? high { get; set; }
        [JsonProperty("low")]
        public double? low { get; set; }
        [JsonProperty("close")]
        public double? close { get; set; }
        [JsonProperty("trades")]
        public double? trades { get; set; }
        [JsonProperty("volume")]
        public double? volume { get; set; }
        [JsonProperty("vwap")]
        public double? vwap { get; set; }
        [JsonProperty("lastSize")]
        public double? lastSize { get; set; }
        [JsonProperty("turnover")]
        public double? turnover { get; set; }
        [JsonProperty("homeNotional")]
        public double? homeNotional { get; set; }
        [JsonProperty("foreignNotional")]
        public double? foreignNotional { get; set; }


        public CandleData()
        {

        }
    }
}
