using LibraryManager.E2E.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace LibraryManager.E2E.Tests;

public static class DependencyConfig
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();

        // injeta a mesma instância já iniciada
        services.AddSingleton<E2EFactory>(_ => E2ETestContext.Factory);

        return services;
    }
}