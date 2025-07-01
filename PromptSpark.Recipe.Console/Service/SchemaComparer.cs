namespace ConsoleApp1.Service
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;

    public static class SchemaComparer
    {
        public static List<string> CompareSchemas(object schema1, object schema2)
        {
            // Convert both inputs to JSON strings
            string schema1Json = schema1 is string ? schema1.ToString() : JsonConvert.SerializeObject(schema1, Formatting.Indented);
            string schema2Json = schema2 is string ? schema2.ToString() : JsonConvert.SerializeObject(schema2, Formatting.Indented);

            // Parse the schemas into JObject for comparison
            var schema1JObject = JsonConvert.DeserializeObject<JObject>(schema1Json);
            var schema2JObject = JsonConvert.DeserializeObject<JObject>(schema2Json);

            var differences = new List<string>();
            CompareJTokens(schema1JObject, schema2JObject, differences, string.Empty);

            return differences;
        }


        private static void CompareJTokens(JToken token1, JToken token2, List<string> differences, string path)
        {
            if (token1 == null && token2 == null) return;

            if (token1 == null || token2 == null)
            {
                differences.Add($"{path}: One of the tokens is null.");
                return;
            }

            if (!JToken.DeepEquals(token1, token2))
            {
                if (token1.Type != token2.Type)
                {
                    differences.Add($"{path}: Type mismatch (Schema1: {token1.Type}, Schema2: {token2.Type})");
                    return;
                }

                if (token1.Type == JTokenType.Object)
                {
                    var obj1 = (JObject)token1;
                    var obj2 = (JObject)token2;

                    var allKeys = new HashSet<string>(obj1.Properties().Select(p => p.Name));
                    allKeys.UnionWith(obj2.Properties().Select(p => p.Name));

                    foreach (var key in allKeys)
                    {
                        CompareJTokens(obj1[key], obj2[key], differences, $"{path}.{key}");
                    }
                }
                else if (token1.Type == JTokenType.Array)
                {
                    var array1 = (JArray)token1;
                    var array2 = (JArray)token2;

                    var maxLength = Math.Max(array1.Count, array2.Count);
                    for (int i = 0; i < maxLength; i++)
                    {
                        CompareJTokens(
                            array1.ElementAtOrDefault(i),
                            array2.ElementAtOrDefault(i),
                            differences,
                            $"{path}[{i}]"
                        );
                    }
                }
                else
                {
                    differences.Add($"{path}: Value mismatch (Schema1: {token1}, Schema2: {token2})");
                }
            }
        }
    }
}


//public class MomRecipeRequestService
//{

//    public static object CreateRequest(string userContent)
//    {
//        return new
//        {
//            model = "gpt-4o",
//            messages = new[]
//            {
//                new { role = "system", content = "You are MOM Recipe. Write an easy-to-read, easy-to-make recipe..." },
//                new { role = "user", content = userContent }
//            },
//            temperature = 0.8,
//            response_format = new
//            {
//                type = "json_schema",
//                json_schema = new
//                {
//                    name = "mom_recipe",
//                    schema = MomRecipeSchema,
//                    strict = true
//                }
//            }
//        };
//    }

//    public static object MomRecipeSchema => new
//    {
//        type = "object",
//        properties = new
//        {
//            Name = new { type = "string" },
//            Description = new { type = "string" },
//            Category = new { type = "string" },
//            Ingredients = new { type = "array", items = new { type = "string" } },
//            Instructions = new { type = "array", items = new { type = "string" } },
//            Servings = new { type = "integer" },
//            SEO_Keywords = new { type = "array", items = new { type = "string" } }
//        },
//        required = new[]
//        {
//            "Name", "Description", "Category", "Ingredients",
//            "Instructions", "Servings", "SEO_Keywords"
//        },
//        additionalProperties = false
//    };

//    public class RecipeModelAI
//    {

//        [JsonProperty("Category")]
//        public string Category { get; set; }

//        [JsonProperty("Description")]
//        public string Description { get; set; }

//        [JsonProperty("Ingredients")]
//        public List<string> Ingredients { get; set; }

//        [JsonProperty("Instructions")]
//        public List<string> Instructions { get; set; }
//        [JsonProperty("Name")]
//        public string Name { get; set; }

//        [JsonProperty("SEO_Keywords")]
//        public List<string> SeoKeywords { get; set; }

//        [JsonProperty("Servings")]
//        public int Servings { get; set; }
//    }

//    public class ChatCompletionResponse
//    {

//        [JsonProperty("choices")]
//        public List<Choice> Choices { get; set; }

//        [JsonProperty("created")]
//        public long Created { get; set; }
//        [JsonProperty("id")]
//        public string Id { get; set; }

//        [JsonProperty("model")]
//        public string Model { get; set; }

//        [JsonProperty("object")]
//        public string Object { get; set; }

//        [JsonProperty("system_fingerprint")]
//        public string SystemFingerprint { get; set; }

//        [JsonProperty("usage")]
//        public Usage Usage { get; set; }
//    }

//    public class Choice
//    {

//        [JsonProperty("finish_reason")]
//        public string FinishReason { get; set; }
//        [JsonProperty("index")]
//        public int Index { get; set; }

//        [JsonProperty("logprobs")]
//        public object LogProbs { get; set; }

//        [JsonProperty("message")]
//        public Message Message { get; set; }
//    }

//    public class Message
//    {

//        [JsonProperty("content")]
//        public string Content { get; set; }

//        [JsonProperty("refusal")]
//        public object Refusal { get; set; }
//        [JsonProperty("role")]
//        public string Role { get; set; }
//    }

//    public class Usage
//    {

//        [JsonProperty("completion_tokens")]
//        public int CompletionTokens { get; set; }

//        [JsonProperty("completion_tokens_details")]
//        public TokenDetails CompletionTokensDetails { get; set; }
//        [JsonProperty("prompt_tokens")]
//        public int PromptTokens { get; set; }

//        [JsonProperty("prompt_tokens_details")]
//        public TokenDetails PromptTokensDetails { get; set; }

//        [JsonProperty("total_tokens")]
//        public int TotalTokens { get; set; }
//    }

//    public class TokenDetails
//    {

//        [JsonProperty("accepted_prediction_tokens")]
//        public int AcceptedPredictionTokens { get; set; }

//        [JsonProperty("audio_tokens")]
//        public int AudioTokens { get; set; }
//        [JsonProperty("cached_tokens")]
//        public int CachedTokens { get; set; }

//        [JsonProperty("reasoning_tokens")]
//        public int ReasoningTokens { get; set; }

//        [JsonProperty("rejected_prediction_tokens")]
//        public int RejectedPredictionTokens { get; set; }
//    }
//}