using HackerNewsReader.Api.Clients;

namespace HackerNewsReader.Api.Tests.Clients;

public sealed class HackerNewsClientTests_Integration
{
    private HttpClient _httpClient;
    private HackerNewsClient _client;

    [SetUp]
    public void SetUp()
    {
        _httpClient = new HttpClient() { BaseAddress = new Settings().HackerNewsApiUrl };
        _client = new HackerNewsClient(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    [Test]
    public async Task GetBestStories()
    {
        var response = await _client.GetBestStories();
        
        Assert.AreEqual(200, response.Length);
    }

    [Test]
    public async Task GetItem()
    {
        var response = await _client.GetItem(41775463);
        
        Assert.LessOrEqual(145, response.Kids?.Length ?? 0);
        Assert.AreEqual("drpossum", response.By);
        Assert.AreEqual(1728381166, response.Time);
        
        // not sure if it can go to 0 over time
        Assert.LessOrEqual(1, response.Score);
        Assert.AreEqual("Nobel Prize in Physics awarded to John Hopfield and Geoffrey Hinton [pdf]", response.Title);
        Assert.AreEqual("https://www.nobelprize.org/uploads/2024/09/advanced-physicsprize2024.pdf", response.Url);
    }
    
    [Test]
    public async Task GetBestStoriesAreSortedInDescendingScoreOrder()
    {
        var response = await _client.GetBestStories();
        var stories = await Task.WhenAll(response.Select(_client.GetItem));

        for (var i = 1; i < stories.Length; ++i)
        {
            Assert.GreaterOrEqual(stories[i-1].Score, stories[i].Score);
        }
        
        Assert.AreEqual(200, stories.Length);
    }
}