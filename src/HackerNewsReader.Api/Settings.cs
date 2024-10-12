namespace HackerNewsReader.Api;

public sealed class Settings
{
    public Uri HackerNewsApiUrl { get; set; } = new ("https://hacker-news.firebaseio.com/");
    public TimeSpan HackerNewsApiTimeout { get; set; }
    public int HackerNewsApiMaxConcurrentConnections { get; set; }
    public TimeSpan BestStoryIdsRefreshInterval { get; set; }
    public TimeSpan BestStoryIdsFailureRefreshInterval { get; set; }
    public TimeSpan StoryRefreshInterval { get; set; }
    public TimeSpan StoryFailureRefreshInterval { get; set; }
}