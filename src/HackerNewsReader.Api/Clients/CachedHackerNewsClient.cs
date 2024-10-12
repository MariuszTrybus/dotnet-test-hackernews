using System.Collections.Concurrent;

namespace HackerNewsReader.Api.Clients;

public sealed class CachedHackerNewsClient : IHackerNewsClient
{
    private readonly IHackerNewsClient _client;
    private readonly ReadThroughCache<int[]> _bestStoryIds;
    private readonly ConcurrentDictionary<int, ReadThroughCache<HackerNewsItem>> _items = new();
    private readonly Settings _settings;
    private readonly ILogger<CachedHackerNewsClient> _logger;

    public CachedHackerNewsClient(IHackerNewsClient client, Settings settings, ILogger<CachedHackerNewsClient> logger)
    {
        _client = client;
        _settings = settings;
        _logger = logger;
        _bestStoryIds = new ReadThroughCache<int[]>(
            async () =>
            {
                int[] result;
                try
                {
                    result = await client.GetBestStories();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Could not fetch best stories from HackerNews API");
                    throw;
                }
                    
                foreach (var key in _items.Keys.Where(x => !result.Contains(x)))
                {
                    _items.Remove(key, out _);
                }
                    
                return result;
            },
            settings.BestStoryIdsRefreshInterval,
            settings.BestStoryIdsFailureRefreshInterval,
            true);
    }
        
    public Task<int[]> GetBestStories()
    {
        return _bestStoryIds.Get();
    }

    public Task<HackerNewsItem> GetItem(int id)
    {
        var cache = _items.GetOrAdd(id, new ReadThroughCache<HackerNewsItem>(
            async () =>
            {
                try
                {
                    return await _client.GetItem(id);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Could not fetch story {Id} details from HackerNews API", id);
                    throw;
                }
            },
            _settings.StoryRefreshInterval,
            _settings.StoryFailureRefreshInterval,
            false));
            
        return cache.Get();
    }
}