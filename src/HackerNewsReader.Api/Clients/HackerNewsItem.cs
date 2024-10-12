using System.Text.Json.Serialization;

namespace HackerNewsReader.Api.Clients;

public sealed class HackerNewsItem
{
    [JsonPropertyName("by")]
    public string? By { get; set; }
    
    [JsonPropertyName("score")]
    public int Score { get; set; }
    
    [JsonPropertyName("time")]
    public int Time { get; set; }
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    [JsonPropertyName("kids")]
    public int[]? Kids { get; set; }
}