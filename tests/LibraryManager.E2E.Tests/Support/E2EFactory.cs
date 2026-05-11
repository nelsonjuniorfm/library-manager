using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace LibraryManager.E2E.Tests.Support;

public sealed class E2EFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:7.0").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDB:ConnectionString"] = _mongo.GetConnectionString(),
                ["MongoDB:Database"] = "librarymanager_e2e"
            });
        });
    }

    public async Task ResetAsync()
    {
        var client = new MongoClient(_mongo.GetConnectionString());
        var db = client.GetDatabase("librarymanager_e2e");
        await db.DropCollectionAsync("books");
        await db.DropCollectionAsync("members");
        await db.DropCollectionAsync("loans");
    }

    public async Task InitializeAsync() => await _mongo.StartAsync();
    public new async Task DisposeAsync() => await _mongo.DisposeAsync();
}