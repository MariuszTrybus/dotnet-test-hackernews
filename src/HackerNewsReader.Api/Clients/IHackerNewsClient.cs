namespace HackerNewsReader.Api.Clients;

public interface IHackerNewsClient
{
    Task<int[]> GetBestStories();
    Task<HackerNewsItem> GetItem(int itemId);
}