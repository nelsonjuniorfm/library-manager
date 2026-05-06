using LibraryManager.Domain.ValueObjects;
using LibraryManager.Domain.Entities;
using LibraryManager.Infrastructure.Persistence;
using LibraryManager.Integration.Tests.Fixtures;
using LibraryManager.TestHelpers.Fakers;
using Shouldly;

namespace LibraryManager.Integration.Tests.Repositories;

[Collection("MongoDB")]
public class MongoBookRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbFixture _fixture;
    private readonly MongoBookRepository _sut;

    public MongoBookRepositoryTests(MongoDbFixture fixture)
    {
        _fixture = fixture;
        _sut = new MongoBookRepository(fixture.Database);
    }

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_WhenCalled_PersistsBook()
    {
        var book = BookFaker.Valid();

        await _sut.AddAsync(book);
        var found = await _sut.GetByIdAsync(book.Id);

        found.ShouldNotBeNull();
        found.Title.ShouldBe(book.Title);
        found.ISBN.Value.ShouldBe(book.ISBN.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByISBNAsync_WhenExists_ReturnsBook()
    {
        var book = BookFaker.Valid();
        await _sut.AddAsync(book);

        var found = await _sut.GetByISBNAsync(book.ISBN.Value);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(book.Id);
    }

    [Fact]
    public async Task GetByISBNAsync_WhenNotFound_ReturnsNull()
    {
        var result = await _sut.GetByISBNAsync("000-0-00-000000-0");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenCopiesChange_PersistsNewValue()
    {
        var book = BookFaker.Valid(totalCopies: 3);
        await _sut.AddAsync(book);

        book.Reserve(); // AvailableCopies = 2
        await _sut.UpdateAsync(book);

        var found = await _sut.GetByIdAsync(book.Id);
        found!.AvailableCopies.ShouldBe(2);
    }

    [Fact]
    public async Task SearchAsync_ByTitle_ReturnMatchingBooks()
    {
        var book = new Book(
            new ISBN("978-3-16-148410-0"),
            "Clean Code",
            "Robert C. Martin",
            2);

        await _sut.AddAsync(book);

        var results = await _sut.SearchAsync("clean");

        results.ShouldHaveSingleItem();
        results.First().Title.ShouldBe("Clean Code");
    }

    [Fact]
    public async Task SearchAsync_ByAuthor_ReturnsMatchingBooks()
    {
        var book = new Book(
            new ISBN("978-3-16-148410-0"),
            "Clean Code",
            "Robert C. Martin",
            2);

        await _sut.AddAsync(book);

        var results = await _sut.SearchAsync("martin");

        results.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task SearchAsync_WhenNoMatch_ReturnsEmpty()
    {
        var results = await _sut.SearchAsync("xyzabcnaoexiste");

        results.ShouldBeEmpty();
    }
}