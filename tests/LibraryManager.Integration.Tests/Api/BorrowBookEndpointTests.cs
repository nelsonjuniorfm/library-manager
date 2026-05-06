using System.Net;
using System.Net.Http.Json;
using LibraryManager.Integration.Tests.Fixtures;
using Shouldly;

namespace LibraryManager.Integration.Tests.Api;

[Collection("Api")]
public class BorrowBookEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public BorrowBookEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetAsync();
    public Task DisposeAsync()    => Task.CompletedTask;

    [Fact]
    public async Task PostBorrow_WhenValid_Returns201WithLoanId()
    {
        // Arrange — cria livro e membro via API ou direto nos repositórios
        var bookId   = await SeedBookAsync();
        var memberId = await SeedMemberAsync();

        var payload = new { BookId = bookId, MemberId = memberId };

        // Act
        var response = await _client.PostAsJsonAsync("/loans", payload);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<BorrowResponse>();
        body!.LoanId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task PostBorrow_WhenBookNotAvailable_Returns422()
    {
        var bookId   = await SeedBookAsync(totalCopies: 1);
        var member1  = await SeedMemberAsync();
        var member2  = await SeedMemberAsync();

        // Primeiro empréstimo esgota o livro
        await _client.PostAsJsonAsync("/loans",
            new { BookId = bookId, MemberId = member1 });

        // Segundo deve falhar
        var response = await _client.PostAsJsonAsync("/loans",
            new { BookId = bookId, MemberId = member2 });

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostBorrow_WhenBookNotFound_Returns404()
    {
        var memberId = await SeedMemberAsync();
        var payload  = new { BookId = Guid.NewGuid(), MemberId = memberId };

        var response = await _client.PostAsJsonAsync("/loans", payload);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // Helpers de seed
    private async Task<Guid> SeedBookAsync(int totalCopies = 3)
    {
        var payload  = new { ISBN = "978-3-16-148410-0", Title = "Test Book",
                             Author = "Author", TotalCopies = totalCopies };
        var response = await _client.PostAsJsonAsync("/books", payload);
        var body     = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    private async Task<Guid> SeedMemberAsync()
    {
        var payload  = new { Name = "Test Member",
                             Email = $"{Guid.NewGuid()}@test.com" };
        var response = await _client.PostAsJsonAsync("/members", payload);
        var body     = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    private record BorrowResponse(Guid LoanId);
    private record IdResponse(Guid Id);
}