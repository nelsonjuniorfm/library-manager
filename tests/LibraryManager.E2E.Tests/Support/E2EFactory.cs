using LibraryManager.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace LibraryManager.E2E.Tests.Support;

public sealed class E2EFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:7.0")
        .WithName($"librarymanager-api-{Guid.NewGuid()}")
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // remove a conexão real e o banco real
            services.RemoveAll<IMongoClient>();
            services.RemoveAll<IMongoDatabase>();

            // substitui só o que muda: conexão e banco
            services.AddMongoDb(_mongo.GetConnectionString(), "librarymanager_e2e");
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