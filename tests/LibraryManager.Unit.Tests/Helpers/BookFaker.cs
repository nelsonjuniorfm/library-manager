using Bogus;
using LibraryManager.Domain.Entities;
using LibraryManager.Domain.ValueObjects;

namespace LibraryManager.Unit.Tests.Helpers;

public static class BookFaker
{
    private static readonly Faker F = new("pt_BR");

    public static Book Valid(int totalCopies = 3) =>
        new(
            isbn: new ISBN("978-3-16-148410-0"),
            title: F.Commerce.ProductName(),
            author: F.Name.FullName(),
            totalCopies: totalCopies
        );
}

public static class MemberFaker
{
    private static readonly Faker F = new("pt_BR");

    public static Member Valid() =>
        new(
            name: F.Name.FullName(),
            email: new Email(F.Internet.Email())
        );
}

public static class LoanFaker
{
    public static Loan Active(Guid? bookId = null, Guid? memberId = null) =>
        new(
            bookId: bookId ?? Guid.NewGuid(),
            memberId: memberId ?? Guid.NewGuid(),
            borrowedAt: DateTime.UtcNow.AddDays(-3),
            loanDurationDays: 14
        );

    public static Loan Overdue(Guid? bookId = null, Guid? memberId = null) =>
        new(
            bookId: bookId ?? Guid.NewGuid(),
            memberId: memberId ?? Guid.NewGuid(),
            borrowedAt: DateTime.UtcNow.AddDays(-20),
            loanDurationDays: 14
        );
}