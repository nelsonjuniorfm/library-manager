using LibraryManager.Domain.Entities;
using LibraryManager.Domain.ValueObjects;
using LibraryManager.Infrastructure.Persistence.Documents;

namespace LibraryManager.Infrastructure.Persistence.Mappers;

internal static class BookMapper
{
    public static BookDocument ToDocument(Book book) => new()
    {
        Id = book.Id,
        ISBN = book.ISBN.Value,
        Title = book.Title,
        Author = book.Author,
        TotalCopies = book.TotalCopies,
        AvailableCopies = book.AvailableCopies
    };

    public static Book ToEntity(BookDocument doc) =>
        Book.Reconstitute(
            doc.Id,
            new ISBN(doc.ISBN),
            doc.Title,
            doc.Author,
            doc.TotalCopies,
            doc.AvailableCopies);
}