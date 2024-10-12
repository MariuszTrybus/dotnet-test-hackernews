using System.Text.Json.Serialization;

using HackerNewsReader.Api.Clients;

namespace HackerNewsReader.Api.Models;

public sealed class BestStoryResponse
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
    
    [JsonPropertyName("postedBy")]
    public string? PostedBy { get; set; }
    
    [JsonPropertyName("time")]
    public DateTimeOffset Time { get; set; }
    
    [JsonPropertyName("score")]
    public int Score { get; set; }
    
    [JsonPropertyName("commentCount")]
    public int CommentCount { get; set; }

    public static BestStoryResponse From(HackerNewsItem item)
    {
        return new BestStoryResponse
        {
            Score = item.Score,
            Time = DateTimeOffset.FromUnixTimeSeconds(item.Time).UtcDateTime,
            Uri = item.Url,
            Title = item.Title,
            CommentCount = item.Kids?.Length ?? 0,
            PostedBy = item.By
        };
    }
}