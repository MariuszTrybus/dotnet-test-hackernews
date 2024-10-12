using HackerNewsReader.Api.Models;
using HackerNewsReader.Api.Services;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace HackerNewsReader.Api.Tests.Controllers;

public sealed class HackerNewsReaderControllerTests
{
    private WebApplicationFactory<Program> _factory;
    private Mock<IHackerNewsReaderService> _service;
    private HttpClient _client;

    [SetUp]
    public void SetUp()
    {
        _service = new Mock<IHackerNewsReaderService>();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(x => x.ConfigureServices(services =>
            {
                services.AddSingleton<IHackerNewsReaderService>(_service.Object);
            }));
        _client = _factory.CreateClient();
    }

    [TearDown]
    public async Task TearDown()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }
    
    [Test]
    public async Task InvalidUrl()
    {
        var client = _factory.CreateClient();
        using var response = await client.GetAsync("foo");
        
        Assert.AreEqual(@"HTTP/1.1 404 Not Found

".ToLf(),
            await response.ToFormattedString());
    }
    
    [Test]
    public async Task AllowsNInRange1_200([Values(1, 100, 200)]int n)
    {
        _service.Setup(x => x.GetBestStories(It.IsAny<int>()))
            .ReturnsAsync(new[]
            {
                new BestStoryResponse
                {
                    Score = 10,
                    Time = new DateTime(2024, 11, 12, 18, 19, 20, DateTimeKind.Utc),
                    Title = "some title 1",
                    PostedBy = "some author 1",
                    CommentCount = 3,
                    Uri = "http://some-url/1"
                }
            });
        
        var client = _factory.CreateClient();
        using var response = await client.GetAsync($"beststories?n={n}");
        
        Assert.AreEqual(
            @"HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

[{""title"":""some title 1"",""uri"":""http://some-url/1"",""postedBy"":""some author 1"",""time"":""2024-11-12T18:19:20+00:00"",""score"":10,""commentCount"":3}]".ToLf(),
            await response.ToFormattedString());
        
        _service.Verify(x => x.GetBestStories(n), Times.Once);
    }
    
    [Test]
    public async Task ReturnBestStoriesInExpectedFormat()
    {
        _service.Setup(x => x.GetBestStories(It.IsAny<int>()))
            .ReturnsAsync(new[]
            {
                new BestStoryResponse
                {
                    Score = 10,
                    Time = new DateTime(2024, 11, 12, 18, 19, 20, DateTimeKind.Utc),
                    Title = "some title 1",
                    PostedBy = "some author 1",
                    CommentCount = 3,
                    Uri = "http://some-url/1"
                },
                new BestStoryResponse
                {
                    Score = 5,
                    Time = new DateTime(2024, 11, 12, 18, 19, 21, DateTimeKind.Utc),
                    Title = "some title 2",
                    PostedBy = "some author 2",
                    CommentCount = 4,
                    Uri = "http://some-url/2"
                }
            });
        
        var client = _factory.CreateClient();
        using var response = await client.GetAsync("beststories?n=100");
        
        Assert.AreEqual(
            @"HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

[{""title"":""some title 1"",""uri"":""http://some-url/1"",""postedBy"":""some author 1"",""time"":""2024-11-12T18:19:20+00:00"",""score"":10,""commentCount"":3},{""title"":""some title 2"",""uri"":""http://some-url/2"",""postedBy"":""some author 2"",""time"":""2024-11-12T18:19:21+00:00"",""score"":5,""commentCount"":4}]".ToLf(),
            await response.ToFormattedString());
        
        _service.Verify(x => x.GetBestStories(100), Times.Once);
    }
    
    [Test]
    public async Task DefaultsTo200Stories()
    {
        var client = _factory.CreateClient();
        using var response = await client.GetAsync("beststories");
        
        Assert.AreEqual(
            @"HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

[]".ToLf(),
            await response.ToFormattedString());
        _service.Verify(x => x.GetBestStories(200), Times.Once);
    }
    
    [Test]
    public async Task RejectsNOutsideRange1_200([Values(-1, 0, 201)]int n)
    {
        var client = _factory.CreateClient();
        using var response = await client.GetAsync($"beststories?n={n}");

        Assert.AreEqual(
            @"HTTP/1.1 400 Bad Request
Content-Type: application/problem+json; charset=utf-8

{""type"":""https://tools.ietf.org/html/rfc9110#section-15.5.1"",""title"":""One or more validation errors occurred."",""status"":400,""errors"":{""n"":[""The field n must be between 1 and 200.""]},""traceId"":""fake-trace-id""}".ToLf(),
            (await response.ToFormattedString()).ReplaceTraceId());
        
        _service.Verify(x => x.GetBestStories(It.IsAny<int>()), Times.Never);
    }
}