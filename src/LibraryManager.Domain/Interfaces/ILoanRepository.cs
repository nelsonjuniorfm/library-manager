using LibraryManager.Domain.Entities;

namespace LibraryManager.Domain.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Loan>> GetActiveLoansByMemberAsync(Guid memberId, CancellationToken ct = default);
    Task AddAsync(Loan loan, CancellationToken ct = default);
    Task UpdateAsync(Loan loan, CancellationToken ct = default);
}