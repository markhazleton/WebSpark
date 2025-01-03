using Newtonsoft.Json;
using System;

namespace ConsoleApp1.Service
{
    public class MathReasoningResponse
    {
        [JsonProperty("steps")]
        public Step[] Steps { get; set; }

        [JsonProperty("final_answer")]
        public string FinalAnswer { get; set; }
    }
}
