// Support/Hooks.cs
using LibraryManager.E2E.Tests.Support;
using Reqnroll;

namespace LibraryManager.E2E.Tests;

[Binding]
public sealed class Hooks
{
    private readonly E2EFactory _factory;

    public Hooks(E2EFactory factory) => _factory = factory;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
        => await E2ETestContext.Factory.InitializeAsync();

    [AfterTestRun]
    public static async Task AfterTestRun()
        => await E2ETestContext.Factory.DisposeAsync();

    [BeforeScenario]
    public async Task BeforeScenario()
        => await _factory.ResetAsync();
}