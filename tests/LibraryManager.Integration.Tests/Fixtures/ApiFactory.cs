using LibraryManager.Infrastructure;
using MongoDB.Driver;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MongoDb;

namespace LibraryManager.Integration.Tests.Fixtures;

// Factory separada para os testes de API — também sobe um container próprio
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder()
        .WithImage("mongo:7.0")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Sobrescreve a config da aplicação com o container de teste
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDB:ConnectionString"] = _mongo.GetConnectionString(),
                ["MongoDB:Database"] = "librarymanager_integration"
            });
        });
    }

    public async Task ResetAsync()
    {
        var client = new MongoClient(_mongo.GetConnectionString());
        var db = client.GetDatabase("librarymanager_integration");
        await db.DropCollectionAsync("books");
        await db.DropCollectionAsync("members");
        await db.DropCollectionAsync("loans");
    }

    public async Task InitializeAsync()
    {
        var maxRetries = 3;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await _mongo.StartAsync();
                return;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                Console.WriteLine($"Tentativa {i + 1} falhou, retrying: {ex.Message}");
            }
        }
    }
    public new async Task DisposeAsync() => await _mongo.DisposeAsync();
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFactory> { }