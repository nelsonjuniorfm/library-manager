// UseCases/BorrowBook/BorrowBookHandler.cs
using LibraryManager.Application.Common;
using LibraryManager.Domain.Entities;
using LibraryManager.Domain.Interfaces;

namespace LibraryManager.Application.UseCases.BorrowBook;

public sealed class BorrowBookHandler : IHandler<BorrowBookCommand, Guid>
{
    private readonly IBookRepository   _books;
    private readonly IMemberRepository _members;
    private readonly ILoanRepository   _loans;

    public BorrowBookHandler(
        IBookRepository   books,
        IMemberRepository members,
        ILoanRepository   loans)
    {
        _books   = books;
        _members = members;
        _loans   = loans;
    }

    public async Task<Guid> Handle(BorrowBookCommand cmd, CancellationToken ct)
    {
        // 1. Busca — lança KeyNotFoundException se não encontrar
        var book = await _books.GetByIdAsync(cmd.BookId, ct)
            ?? throw new KeyNotFoundException($"Book '{cmd.BookId}' not found.");

        var member = await _members.GetByIdAsync(cmd.MemberId, ct)
            ?? throw new KeyNotFoundException($"Member '{cmd.MemberId}' not found.");

        // 2. Valida regras de domínio — pode lançar exceções de domínio
        member.CanBorrow();

        // 3. Muta estado das entidades
        book.Reserve();
        member.IncrementLoans();

        // 4. Cria o empréstimo
        var loan = new Loan(book.Id, member.Id, DateTime.UtcNow);

        // 5. Persiste tudo
        await _loans.AddAsync(loan, ct);
        await _books.UpdateAsync(book, ct);
        await _members.UpdateAsync(member, ct);

        return loan.Id;
    }
}