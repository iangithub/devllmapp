using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WithoutSkSample.GPT4
{
    public class Completion
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "object")]
        public string AiType { get; set; }
        [JsonProperty(PropertyName = "created")]
        public int Created { get; set; }
        [JsonProperty(PropertyName = "model")]
        public string Model { get; set; }
        [JsonProperty(PropertyName = "choices")]
        public List<Choices> Choices { get; set; }
        [JsonProperty(PropertyName = "usage")]
        public Usage Usage { get; set; }
    }

    public class Choices
    {
        [JsonProperty(PropertyName = "message")]
        public Message Message { get; set; }
        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }
        [JsonProperty(PropertyName = "logprobs")]
        public object Logprobs { get; set; }
        [JsonProperty(PropertyName = "finish_reason")]
        public string Finish_reason { get; set; }
    }

    public class Usage
    {
        [JsonProperty(PropertyName = "prompt_tokens")]
        public int Prompt_Tokens { get; set; }
        [JsonProperty(PropertyName = "completion_tokens")]
        public int Completion_Tokens { get; set; }
        [JsonProperty(PropertyName = "total_tokens")]
        public int Total_Tokens { get; set; }
    }
}
