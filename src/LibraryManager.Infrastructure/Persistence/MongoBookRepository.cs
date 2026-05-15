using LibraryManager.Domain.Entities;
using LibraryManager.Domain.Interfaces;
using LibraryManager.Infrastructure.Persistence.Documents;
using LibraryManager.Infrastructure.Persistence.Mappers;
using MongoDB.Driver;

namespace LibraryManager.Infrastructure.Persistence;
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class MongoBookRepository : IBookRepository
{
    private readonly IMongoCollection<BookDocument> _collection;

    public MongoBookRepository(IMongoDatabase database)
        => _collection = database.GetCollection<BookDocument>("books");

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(b => b.Id == id)
            .FirstOrDefaultAsync(ct);

        return doc is null ? null : BookMapper.ToEntity(doc);
    }

    public async Task<Book?> GetByISBNAsync(string isbn, CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(b => b.ISBN == isbn)
            .FirstOrDefaultAsync(ct);

        return doc is null ? null : BookMapper.ToEntity(doc);
    }

    public async Task<IEnumerable<Book>> SearchAsync(string term, CancellationToken ct = default)
    {
        var filter = Builders<BookDocument>.Filter.Or(
            Builders<BookDocument>.Filter.Regex(b => b.Title,
                new MongoDB.Bson.BsonRegularExpression(term, "i")),
            Builders<BookDocument>.Filter.Regex(b => b.Author,
                new MongoDB.Bson.BsonRegularExpression(term, "i"))
        );

        var docs = await _collection.Find(filter).ToListAsync(ct);
        return docs.Select(BookMapper.ToEntity);
    }

    public async Task AddAsync(Book book, CancellationToken ct = default)
        => await _collection.InsertOneAsync(BookMapper.ToDocument(book), null, ct);

    public async Task UpdateAsync(Book book, CancellationToken ct = default)
        => await _collection.ReplaceOneAsync(
            b => b.Id == book.Id,
            BookMapper.ToDocument(book),
            new ReplaceOptions { IsUpsert = false },
            ct);
}