using LibraryManager.Domain.Interfaces;
using LibraryManager.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace LibraryManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString is not configured.");

        var databaseName = configuration["MongoDB:Database"]
            ?? throw new InvalidOperationException("MongoDB:Database is not configured.");

        services.AddMongoDb(connectionString, databaseName);
        services.AddRepositories();

        return services;
    }

    // método público para os testes sobrescreverem só a conexão
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        string connectionString,
        string databaseName)
    {
        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        services.AddScoped(sp => sp.GetRequiredService<IMongoClient>()
            .GetDatabase(databaseName));

        return services;
    }

    // repositórios em método separado — nunca precisam ser reescritos
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IBookRepository, MongoBookRepository>();
        services.AddScoped<IMemberRepository, MongoMemberRepository>();
        services.AddScoped<ILoanRepository, MongoLoanRepository>();

        return services;
    }
}