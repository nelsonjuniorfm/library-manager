using System.Net;
using System.Net.Http.Json;
using LibraryManager.Integration.Tests.Fixtures;
using Shouldly;

namespace LibraryManager.Integration.Tests.Api;

[Collection("Api")]
public class SearchBooksEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public SearchBooksEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetAsync();
    public Task DisposeAsync()    => Task.CompletedTask;

    [Fact]
    public async Task GetBooks_WhenTermMatches_ReturnsResults()
    {
        await _client.PostAsJsonAsync("/books", new
        {
            ISBN       = "978-3-16-148410-0",
            Title      = "Domain-Driven Design",
            Author     = "Eric Evans",
            TotalCopies = 2
        });

        var response = await _client.GetAsync("/books?term=domain");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var books = await response.Content
            .ReadFromJsonAsync<List<BookResponse>>();

        books!.Count.ShouldBe(1);
        books[0].Title.ShouldBe("Domain-Driven Design");
    }

    [Fact]
    public async Task GetBooks_WhenNoMatch_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/books?term=xyzabcnaoexiste");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var books = await response.Content
            .ReadFromJsonAsync<List<BookResponse>>();

        books.ShouldBeEmpty();
    }

    private record BookResponse(Guid Id, string Title, string Author,
        string ISBN, int AvailableCopies, int TotalCopies);
}