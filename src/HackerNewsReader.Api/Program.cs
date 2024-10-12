using HackerNewsReader.Api;
using HackerNewsReader.Api.Clients;
using HackerNewsReader.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var settings = new Settings
{
    HackerNewsApiTimeout = TimeSpan.FromSeconds(60),
    HackerNewsApiMaxConcurrentConnections = 10,
    BestStoryIdsRefreshInterval = TimeSpan.FromSeconds(10),
    BestStoryIdsFailureRefreshInterval = TimeSpan.FromSeconds(1),
    StoryRefreshInterval = TimeSpan.FromSeconds(10),
    StoryFailureRefreshInterval = TimeSpan.FromSeconds(1)
};

builder.Services.AddControllers();
builder.Services
    .AddHttpClient<HackerNewsClient>(client =>
    {
        client.BaseAddress = settings.HackerNewsApiUrl;
        client.Timeout = settings.HackerNewsApiTimeout;
    })
    .ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler
    {
        MaxConnectionsPerServer = settings.HackerNewsApiMaxConcurrentConnections
    });

builder.Services
    .AddSingleton(settings)
    .AddSingleton<IHackerNewsClient>(x => new CachedHackerNewsClient(x.GetRequiredService<HackerNewsClient>(), x.GetRequiredService<Settings>(), x.GetRequiredService<ILogger<CachedHackerNewsClient>>()))
    .AddTransient<IHackerNewsReaderService, HackerNewsReaderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

namespace HackerNewsReader.Api
{
    public partial class Program { }
}
