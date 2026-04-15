using LibraryManager.Domain.Entities;

namespace LibraryManager.Domain.Interfaces;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Member?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(Member member, CancellationToken ct = default);
    Task UpdateAsync(Member member, CancellationToken ct = default);
}