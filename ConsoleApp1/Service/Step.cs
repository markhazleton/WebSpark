using Newtonsoft.Json;
using System;

namespace ConsoleApp1.Service
{
    public class Step
    {
        [JsonProperty("explanation")]
        public string Explanation { get; set; }

        [JsonProperty("output")]
        public string Output { get; set; }
    }
}
