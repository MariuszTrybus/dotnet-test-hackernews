using HackerNewsReader.Api.Clients;
using HackerNewsReader.Api.Models;

namespace HackerNewsReader.Api.Services;

public sealed class HackerNewsReaderService(IHackerNewsClient client, ILogger<HackerNewsReaderService> logger) : IHackerNewsReaderService
{
    public async Task<BestStoryResponse[]> GetBestStories(int numStories)
    {
        var bestStories = await client.GetBestStories();
        var results = await Task.WhenAll(bestStories
            .Take(numStories)
            .Select(async x =>
            {
                try
                {
                    var item = await client.GetItem(x);
                    return BestStoryResponse.From(item);
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Could not fetch story {Id}, skipping it in response", x);
                    return null;
                }
            }));
        
        return results
            .Where(x => x != null)
            .ToArray();
    }
}