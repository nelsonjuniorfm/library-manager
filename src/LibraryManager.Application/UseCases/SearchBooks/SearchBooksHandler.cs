// UseCases/SearchBooks/SearchBooksHandler.cs
using LibraryManager.Application.Common;
using LibraryManager.Domain.Interfaces;

namespace LibraryManager.Application.UseCases.SearchBooks;

public sealed class SearchBooksHandler
    : IHandler<SearchBooksQuery, IEnumerable<BookResult>>
{
    private readonly IBookRepository _books;

    public SearchBooksHandler(IBookRepository books) => _books = books;

    public async Task<IEnumerable<BookResult>> Handle(
        SearchBooksQuery query, CancellationToken ct)
    {
        var books = await _books.SearchAsync(query.Term, ct);

        return books.Select(b => new BookResult(
            b.Id,
            b.ISBN.Value,
            b.Title,
            b.Author,
            b.AvailableCopies,
            b.TotalCopies));
    }
}