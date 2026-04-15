using LibraryManager.Domain.Entities;

namespace LibraryManager.Domain.Interfaces;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Book?> GetByISBNAsync(string isbn, CancellationToken ct = default);
    Task<IEnumerable<Book>> SearchAsync(string term, CancellationToken ct = default);
    Task AddAsync(Book book, CancellationToken ct = default);
    Task UpdateAsync(Book book, CancellationToken ct = default);
}