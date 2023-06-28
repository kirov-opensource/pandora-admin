using System.Text.Json.Serialization;

namespace Pandora.Admin.WebAPI.Models;

public class GetConversationResponseModel
{
    [JsonPropertyName("items")]
    public List<ConversationItem> Items { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("has_missing_conversations")]
    public bool HasMissingConversations { get; set; }
    
}
public class ConversationItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("create_time")]
    public DateTime CreateTime { get; set; }

    [JsonPropertyName("update_time")]
    public DateTime UpdateTime { get; set; }

    [JsonPropertyName("mapping")]
    public object Mapping { get; set; }

    [JsonPropertyName("current_node")]
    public object CurrentNode { get; set; }
}