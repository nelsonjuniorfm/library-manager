using LibraryManager.Domain.Entities;
using LibraryManager.Domain.Enums;
using LibraryManager.Infrastructure.Persistence.Documents;

namespace LibraryManager.Infrastructure.Persistence.Mappers;

internal static class LoanMapper
{
    public static LoanDocument ToDocument(Loan loan) => new()
    {
        Id         = loan.Id,
        BookId     = loan.BookId,
        MemberId   = loan.MemberId,
        BorrowedAt = loan.BorrowedAt,
        DueDate    = loan.DueDate,
        ReturnedAt = loan.ReturnedAt,
        Status     = loan.Status.ToString()
    };

    public static Loan ToEntity(LoanDocument doc) =>
        Loan.Reconstitute(
            doc.Id,
            doc.BookId,
            doc.MemberId,
            doc.BorrowedAt,
            doc.DueDate,
            doc.ReturnedAt,
            Enum.Parse<LoanStatus>(doc.Status));
}