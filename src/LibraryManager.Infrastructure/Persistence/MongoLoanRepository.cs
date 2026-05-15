using LibraryManager.Domain.Entities;
using LibraryManager.Domain.Interfaces;
using LibraryManager.Infrastructure.Persistence.Documents;
using LibraryManager.Infrastructure.Persistence.Mappers;
using MongoDB.Driver;

namespace LibraryManager.Infrastructure.Persistence;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class MongoLoanRepository : ILoanRepository
{
    private readonly IMongoCollection<LoanDocument> _collection;

    public MongoLoanRepository(IMongoDatabase database)
        => _collection = database.GetCollection<LoanDocument>("loans");

    public async Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(l => l.Id == id)
            .FirstOrDefaultAsync(ct);

        return doc is null ? null : LoanMapper.ToEntity(doc);
    }

    public async Task<IEnumerable<Loan>> GetActiveLoansByMemberAsync(
        Guid memberId, CancellationToken ct = default)
    {
        var docs = await _collection
            .Find(l => l.MemberId == memberId && l.Status == "Active")
            .ToListAsync(ct);

        return docs.Select(LoanMapper.ToEntity);
    }

    public async Task AddAsync(Loan loan, CancellationToken ct = default)
        => await _collection.InsertOneAsync(LoanMapper.ToDocument(loan), null, ct);

    public async Task UpdateAsync(Loan loan, CancellationToken ct = default)
        => await _collection.ReplaceOneAsync(
            l => l.Id == loan.Id,
            LoanMapper.ToDocument(loan),
            new ReplaceOptions { IsUpsert = false },
            ct);
}