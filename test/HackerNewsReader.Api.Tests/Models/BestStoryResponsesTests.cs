using HackerNewsReader.Api.Clients;
using HackerNewsReader.Api.Models;

namespace HackerNewsReader.Api.Tests.Models;

public sealed class BestStoryResponsesTests
{
    [Test]
    public void ConvertHackerNewsItem()
    {
        var value = BestStoryResponse.From(new HackerNewsItem
        {
            Score = 10,
            Time = 1728304658,
            Title = "some title",
            By = "some author",
            Kids = new[] { 1, 3, 5},
            Url = "some url"
        });
        
        Assert.AreEqual(10, value.Score);
        Assert.AreEqual(new DateTime(2024, 10, 7, 12, 37, 38, DateTimeKind.Utc), value.Time.UtcDateTime);
        Assert.AreEqual("some title", value.Title);
        Assert.AreEqual("some author", value.PostedBy);
        Assert.AreEqual(3, value.CommentCount);
        Assert.AreEqual("some url", value.Uri);
    }
    
    [Test]
    public void ConvertHackerNewsItem_WithNullKids()
    {
        var value = BestStoryResponse.From(new HackerNewsItem
        {
            Score = 10,
            Time = 1728304658,
            Title = "some title",
            By = "some author",
            Kids = null,
            Url = "some url"
        });
        
        Assert.AreEqual(10, value.Score);
        Assert.AreEqual(new DateTime(2024, 10, 7, 12, 37, 38, DateTimeKind.Utc), value.Time.UtcDateTime);
        Assert.AreEqual("some title", value.Title);
        Assert.AreEqual("some author", value.PostedBy);
        Assert.AreEqual(0, value.CommentCount);
        Assert.AreEqual("some url", value.Uri);
    }
}