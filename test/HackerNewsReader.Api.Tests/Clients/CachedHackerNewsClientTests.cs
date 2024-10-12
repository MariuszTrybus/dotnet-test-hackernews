using HackerNewsReader.Api.Clients;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace HackerNewsReader.Api.Tests.Clients;

public sealed class CachedHackerNewsClientTests
{
    [Test]
    public async Task GetBestStories_FetchesNewValues()
    {
        var settings = new Settings
        {
            BestStoryIdsRefreshInterval = TimeSpan.Zero
        };
        
        var client = new Mock<IHackerNewsClient>();
        
        var cachedClient =
            new CachedHackerNewsClient(client.Object, settings, NullLogger<CachedHackerNewsClient>.Instance);

        await cachedClient.GetBestStories();
        await cachedClient.GetBestStories();
        client.Verify(x => x.GetBestStories(), Times.Exactly(2));
    }
    
    [Test]
    public async Task GetItem_FetchesNewValues()
    {
        var settings = new Settings
        {
            StoryRefreshInterval = TimeSpan.Zero
        };
        
        var client = new Mock<IHackerNewsClient>();
        
        var cachedClient =
            new CachedHackerNewsClient(client.Object, settings, NullLogger<CachedHackerNewsClient>.Instance);

        await cachedClient.GetItem(1);
        await cachedClient.GetItem(2);
        await cachedClient.GetItem(1);
        await cachedClient.GetItem(2);
        client.Verify(x => x.GetItem(1), Times.Exactly(2));
        client.Verify(x => x.GetItem(2), Times.Exactly(2));
    }
    
    [Test]
    public async Task GetBestStories_CachesValues()
    {
        var settings = new Settings
        {
            BestStoryIdsRefreshInterval = TimeSpan.FromDays(1)
        };
        
        var client = new Mock<IHackerNewsClient>();
        
        var cachedClient =
            new CachedHackerNewsClient(client.Object, settings, NullLogger<CachedHackerNewsClient>.Instance);

        await cachedClient.GetBestStories();
        await cachedClient.GetBestStories();
        client.Verify(x => x.GetBestStories(), Times.Once);
    }
    
    [Test]
    public async Task GetItem_CachesValues()
    {
        var settings = new Settings
        {
            StoryRefreshInterval = TimeSpan.FromDays(1)
        };
        
        var client = new Mock<IHackerNewsClient>();
        
        var cachedClient =
            new CachedHackerNewsClient(client.Object, settings, NullLogger<CachedHackerNewsClient>.Instance);

        await cachedClient.GetItem(1);
        await cachedClient.GetItem(2);
        await cachedClient.GetItem(1);
        await cachedClient.GetItem(2);
        client.Verify(x => x.GetItem(1), Times.Once);
        client.Verify(x => x.GetItem(2), Times.Once);
    }
    
    [Test]
    public async Task RemovesNonBestStoriesFromCache()
    {
        var settings = new Settings { StoryRefreshInterval = TimeSpan.FromDays(1) };
        
        var client = new Mock<IHackerNewsClient>();
        client.Setup(x => x.GetBestStories())
            .ReturnsAsync(new[] { 1, 2 });
        
        var cachedClient =
            new CachedHackerNewsClient(client.Object, settings, NullLogger<CachedHackerNewsClient>.Instance);

        var itemIds = await cachedClient.GetBestStories();
        await Task.WhenAll(itemIds.Select(cachedClient.GetItem));
        
        client.Setup(x => x.GetBestStories())
            .ReturnsAsync(new[] { 1 });
        
        itemIds = await cachedClient.GetBestStories();
        await Task.WhenAll(itemIds.Select(cachedClient.GetItem));
        
        client.Setup(x => x.GetBestStories())
            .ReturnsAsync(new[] { 1, 2 });
        
        itemIds = await cachedClient.GetBestStories();
        await Task.WhenAll(itemIds.Select(cachedClient.GetItem));

        client.Verify(x => x.GetBestStories(), Times.Exactly(3));
        client.Verify(x => x.GetItem(1), Times.Exactly(1));
        client.Verify(x => x.GetItem(2), Times.Exactly(2));
    }
}