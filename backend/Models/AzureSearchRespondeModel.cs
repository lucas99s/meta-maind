using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class AzureSearchResponseModel
    {
        [JsonPropertyName("value")]
        public List<AzureDocument> Value { get; set; }
    }

    public class AzureDocument
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
