using LibraryManager.Application.Common;
using LibraryManager.Domain.Interfaces;

namespace LibraryManager.Application.UseCases.ReturnBook;

public sealed class ReturnBookHandler : IHandler<ReturnBookCommand, bool>
{
    private readonly IBookRepository   _books;
    private readonly IMemberRepository _members;
    private readonly ILoanRepository   _loans;

    public ReturnBookHandler(
        IBookRepository   books,
        IMemberRepository members,
        ILoanRepository   loans)
    {
        _books   = books;
        _members = members;
        _loans   = loans;
    }

    public async Task<bool> Handle(ReturnBookCommand cmd, CancellationToken ct)
    {
        // 1. Busca o empréstimo
        var loan = await _loans.GetByIdAsync(cmd.LoanId, ct)
            ?? throw new KeyNotFoundException($"Loan '{cmd.LoanId}' not found.");

        // 2. Busca livro e membro vinculados ao empréstimo
        var book = await _books.GetByIdAsync(loan.BookId, ct)
            ?? throw new KeyNotFoundException($"Book '{loan.BookId}' not found.");

        var member = await _members.GetByIdAsync(loan.MemberId, ct)
            ?? throw new KeyNotFoundException($"Member '{loan.MemberId}' not found.");

        // 3. Muta estado via domínio
        loan.Return(DateTime.UtcNow);
        book.Release();
        member.DecrementLoans();

        // 4. Persiste os três agregados
        await _loans.UpdateAsync(loan, ct);
        await _books.UpdateAsync(book, ct);
        await _members.UpdateAsync(member, ct);

        return true;
    }
}