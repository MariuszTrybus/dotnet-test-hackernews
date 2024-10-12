using System.ComponentModel.DataAnnotations;

using HackerNewsReader.Api.Models;
using HackerNewsReader.Api.Services;

using Microsoft.AspNetCore.Mvc;

namespace HackerNewsReader.Api.Controllers;

[ApiController]
public sealed class HackerNewsReaderController(IHackerNewsReaderService service) : ControllerBase
{
    [HttpGet("beststories")]
    public Task<BestStoryResponse[]> GetBestStories([FromQuery][Range(1, 200)] int n = 200)
    {
        return service.GetBestStories(n);
    }
}