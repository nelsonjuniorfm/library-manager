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

        services.AddSingleton<IMongoClient>(_ =>
            new MongoClient(connectionString));

        services.AddScoped(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

        services.AddScoped<IBookRepository,   MongoBookRepository>();
        services.AddScoped<IMemberRepository, MongoMemberRepository>();
        services.AddScoped<ILoanRepository,   MongoLoanRepository>();

        return services;
    }
}