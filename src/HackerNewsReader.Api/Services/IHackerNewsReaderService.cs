using HackerNewsReader.Api.Models;

namespace HackerNewsReader.Api.Services;

public interface IHackerNewsReaderService
{
    Task<BestStoryResponse[]> GetBestStories(int numStories);
}