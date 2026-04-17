using LibraryManager.Domain.Interfaces;
using LibraryManager.Unit.Tests.Helpers;
using NSubstitute;
using Shouldly;
using LibraryManager.Application.UseCases.SearchBooks;

namespace LibraryManager.Unit.Tests.Application;

public class SearchBooksHandlerTests
{
    private readonly IBookRepository    _books = Substitute.For<IBookRepository>();
    private readonly SearchBooksHandler _sut;

    public SearchBooksHandlerTests() => _sut = new SearchBooksHandler(_books);

    [Fact]
    public async Task Handle_DelegatesTermToRepository()
    {
        const string term = "Clean Code";
        _books.SearchAsync(term, default).Returns([]);

        await _sut.Handle(new SearchBooksQuery(term), default);

        await _books.Received(1).SearchAsync(term, default);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsEmpty_ReturnsEmptyList()
    {
        _books.SearchAsync(Arg.Any<string>(), default).Returns([]);

        var result = await _sut.Handle(new SearchBooksQuery("qualquer"), default);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsAllBooksFromRepository()
    {
        var books = new[] { BookFaker.Valid(), BookFaker.Valid() };
        _books.SearchAsync(Arg.Any<string>(), default).Returns(books);

        var result = await _sut.Handle(new SearchBooksQuery("algo"), default);

        result.Count().ShouldBe(2);
    }
}