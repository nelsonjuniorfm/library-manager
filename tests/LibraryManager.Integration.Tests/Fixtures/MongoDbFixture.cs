using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace LibraryManager.Integration.Tests.Fixtures;

public sealed class MongoDbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder("mongo:7.0").Build();

    public IMongoDatabase Database { get; private set; } = null!;
    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        
        await _container.StartAsync();

        var client = new MongoClient(ConnectionString);
        Database = client.GetDatabase("librarymanager_tests");
    }

    // Limpa todas as coleções entre testes — mais rápido que recriar o container
    public async Task ResetAsync()
    {
        await Database.DropCollectionAsync("books");
        await Database.DropCollectionAsync("members");
        await Database.DropCollectionAsync("loans");
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}

// Registra a fixture como coleção compartilhada entre classes de teste
[CollectionDefinition("MongoDB")]
public class MongoDbCollection : ICollectionFixture<MongoDbFixture> { }