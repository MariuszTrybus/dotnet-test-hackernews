namespace HackerNewsReader.Api.Clients;

public sealed class HackerNewsClient(HttpClient httpClient) : IHackerNewsClient
{
    public async Task<int[]> GetBestStories()
    {
        var response = await httpClient.GetFromJsonAsync<int[]>("v0/beststories.json");
        return response ?? throw new Exception("Broken HackerNews API contract: null beststories list");
    }

    public async Task<HackerNewsItem> GetItem(int itemId)
    {
        var response = await httpClient.GetFromJsonAsync<HackerNewsItem>($"v0/item/{itemId}.json");
        return response ?? throw new Exception("Broken HackerNews API contract: null item");
    }
}