using HackerNewsReader.Api.Clients;
using HackerNewsReader.Api.Services;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace HackerNewsReader.Api.Tests.Services;

public sealed class HackerNewsReaderServiceTests
{
    [Test]
    [TestCase(0, 3)]
    [TestCase(3, 0)]
    [TestCase(3, 2)]
    [TestCase(3, 4)]
    public async Task ReturnsCorrectNumberOfStories(int numReturned, int numAsked)
    {
        var ids = Enumerable.Range(0, numReturned).ToArray();
        var items = ids
            .Select(x => new HackerNewsItem
            {
                Score = x,
                Kids = Enumerable.Range(1, x + 1).ToArray(),
                By = $"author-{x}",
                Title = $"title-{x}",
                Url = "http://url-{x}",
                Time = (int)DateTimeOffset.UtcNow.AddHours(-x).ToUnixTimeSeconds()
            })
            .ToArray();
        
        var client = new Mock<IHackerNewsClient>();
        client.Setup(x => x.GetBestStories())
            .ReturnsAsync(ids);
        client.Setup(x => x.GetItem(It.IsAny<int>()))
            .ReturnsAsync((int x) => items[x]);
        
        var service = new HackerNewsReaderService(client.Object, NullLogger<HackerNewsReaderService>.Instance);
        var response = await service.GetBestStories(numAsked);
        Assert.AreEqual(Math.Min(numAsked, numReturned), response.Length);
        for (var i = 0; i < response.Length; ++i)
        {
            
            Assert.AreEqual(items[i].Score, response[i].Score);
            Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds(items[i].Time).UtcDateTime, response[i].Time.UtcDateTime);
            Assert.AreEqual(items[i].Title, response[i].Title);
            Assert.AreEqual(items[i].By, response[i].PostedBy);
            Assert.AreEqual(items[i].Kids?.Length ?? 0, response[i].CommentCount);
            Assert.AreEqual(items[i].Url, response[i].Uri);
        }
        
        client.Verify(x => x.GetBestStories(), Times.Once);
        client.Verify(x => x.GetItem(It.IsAny<int>()), Times.Exactly(Math.Min(numAsked, numReturned)));
    }
    
    [Test]
    public async Task SkipsStoryThatErrors()
    {
        var ids = Enumerable.Range(0, 2).ToArray();
        var items = ids
            .Select(x => new HackerNewsItem
            {
                Score = x,
                Kids = Enumerable.Range(1, x + 1).ToArray(),
                By = $"author-{x}",
                Title = $"title-{x}",
                Url = "http://url-{x}",
                Time = (int)DateTimeOffset.UtcNow.AddHours(-x).ToUnixTimeSeconds()
            })
            .ToArray();
        
        var client = new Mock<IHackerNewsClient>();
        client.Setup(x => x.GetBestStories())
            .ReturnsAsync(ids);
        client.Setup(x => x.GetItem(It.IsAny<int>()))
            .ReturnsAsync((int x) => x == 0 ? throw new Exception("test") : items[x]);
        
        var service = new HackerNewsReaderService(client.Object, NullLogger<HackerNewsReaderService>.Instance);
        var response = await service.GetBestStories(2);
        Assert.AreEqual(1, response.Length);
        
        Assert.AreEqual(items[1].Score, response[0].Score);
        Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds(items[1].Time).UtcDateTime, response[0].Time.UtcDateTime);
        Assert.AreEqual(items[1].Title, response[0].Title);
        Assert.AreEqual(items[1].By, response[0].PostedBy);
        Assert.AreEqual(items[1].Kids?.Length ?? 0, response[0].CommentCount);
        Assert.AreEqual(items[1].Url, response[0].Uri);
    }
}