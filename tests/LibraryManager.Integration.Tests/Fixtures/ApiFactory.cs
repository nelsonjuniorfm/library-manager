using LibraryManager.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace LibraryManager.Integration.Tests.Fixtures;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
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
            services.AddMongoDb(_mongo.GetConnectionString(), "librarymanager_integration");
        });
    }

    public async Task ResetAsync()
    {
        var client = new MongoClient(_mongo.GetConnectionString());
        await client.DropDatabaseAsync("librarymanager_integration");
    }

    public async Task InitializeAsync() => await _mongo.StartAsync();
    public new async Task DisposeAsync() => await _mongo.DisposeAsync();
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFactory> { }