using LibraryManager.Domain.Entities;
using LibraryManager.Domain.Interfaces;
using LibraryManager.Infrastructure.Persistence.Documents;
using LibraryManager.Infrastructure.Persistence.Mappers;
using MongoDB.Driver;

namespace LibraryManager.Infrastructure.Persistence;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class MongoMemberRepository : IMemberRepository
{
    private readonly IMongoCollection<MemberDocument> _collection;

    public MongoMemberRepository(IMongoDatabase database)
        => _collection = database.GetCollection<MemberDocument>("members");

    public async Task<Member?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(m => m.Id == id)
            .FirstOrDefaultAsync(ct);

        return doc is null ? null : MemberMapper.ToEntity(doc);
    }

    public async Task<Member?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(m => m.Email == email.ToLowerInvariant())
            .FirstOrDefaultAsync(ct);

        return doc is null ? null : MemberMapper.ToEntity(doc);
    }

    public async Task AddAsync(Member member, CancellationToken ct = default)
        => await _collection.InsertOneAsync(MemberMapper.ToDocument(member), null, ct);

    public async Task UpdateAsync(Member member, CancellationToken ct = default)
        => await _collection.ReplaceOneAsync(
            m => m.Id == member.Id,
            MemberMapper.ToDocument(member),
            new ReplaceOptions { IsUpsert = false },
            ct);
}