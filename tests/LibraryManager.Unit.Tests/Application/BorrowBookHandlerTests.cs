// Application/BorrowBookHandlerTests.cs
using LibraryManager.Domain.Entities;
using LibraryManager.Domain.Exceptions;
using LibraryManager.Domain.Interfaces;
using LibraryManager.Unit.Tests.Helpers;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Shouldly;

// Namespaces que ainda não existem — os testes vão falhar até criarmos
using LibraryManager.Application.UseCases.BorrowBook;

namespace LibraryManager.Unit.Tests.Application;

public class BorrowBookHandlerTests
{
    private readonly IBookRepository   _books   = Substitute.For<IBookRepository>();
    private readonly IMemberRepository _members = Substitute.For<IMemberRepository>();
    private readonly ILoanRepository   _loans   = Substitute.For<ILoanRepository>();
    private readonly BorrowBookHandler _sut;

    public BorrowBookHandlerTests()
        => _sut = new BorrowBookHandler(_books, _members, _loans);

    // --- Caminho feliz ---

    [Fact]
    public async Task Handle_WhenValid_ReturnsLoanId()
    {
        var book   = BookFaker.Valid();
        var member = MemberFaker.Valid();
        var cmd    = new BorrowBookCommand(book.Id, member.Id);

        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        var result = await _sut.Handle(cmd, default);

        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WhenValid_PersistsLoan()
    {
        var book   = BookFaker.Valid();
        var member = MemberFaker.Valid();
        var cmd    = new BorrowBookCommand(book.Id, member.Id);

        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        await _sut.Handle(cmd, default);

        await _loans.Received(1).AddAsync(Arg.Any<Loan>(), default);
    }

    [Fact]
    public async Task Handle_WhenValid_ReservesBookCopy()
    {
        var book   = BookFaker.Valid(totalCopies: 2);
        var member = MemberFaker.Valid();
        var cmd    = new BorrowBookCommand(book.Id, member.Id);

        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        await _sut.Handle(cmd, default);

        book.AvailableCopies.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WhenValid_IncrementsActiveLoanOnMember()
    {
        var book   = BookFaker.Valid();
        var member = MemberFaker.Valid();
        var cmd    = new BorrowBookCommand(book.Id, member.Id);

        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        await _sut.Handle(cmd, default);

        member.ActiveLoans.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesMemberInRepository()
    {
        var book   = BookFaker.Valid();
        var member = MemberFaker.Valid();
        var cmd    = new BorrowBookCommand(book.Id, member.Id);

        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        await _sut.Handle(cmd, default);

        await _members.Received(1).UpdateAsync(member, default);
    }

    // --- Caminhos de erro ---

    [Fact]
    public async Task Handle_WhenBookNotFound_ThrowsKeyNotFoundException()
    {
        var cmd = new BorrowBookCommand(Guid.NewGuid(), Guid.NewGuid());
        _books.GetByIdAsync(cmd.BookId, default).ReturnsNull();

        var act = async () => await _sut.Handle(cmd, default);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenMemberNotFound_ThrowsKeyNotFoundException()
    {
        var book = BookFaker.Valid();
        var cmd  = new BorrowBookCommand(book.Id, Guid.NewGuid());

        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(cmd.MemberId, default).ReturnsNull();

        var act = async () => await _sut.Handle(cmd, default);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenBookNotAvailable_ThrowsBookNotAvailableException()
    {
        var book   = BookFaker.Valid(totalCopies: 1);
        var member = MemberFaker.Valid();
        var cmd    = new BorrowBookCommand(book.Id, member.Id);

        book.Reserve(); // esgota o estoque antes
        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        var act = async () => await _sut.Handle(cmd, default);

        await act.ShouldThrowAsync<BookNotAvailableException>();
    }

    [Fact]
    public async Task Handle_WhenMemberSuspended_ThrowsMemberSuspendedException()
    {
        var book   = BookFaker.Valid();
        var member = MemberFaker.Valid();
        member.Suspend();
        var cmd = new BorrowBookCommand(book.Id, member.Id);

        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        var act = async () => await _sut.Handle(cmd, default);

        await act.ShouldThrowAsync<MemberSuspendedException>();
    }

    [Fact]
    public async Task Handle_WhenLoanLimitReached_ThrowsLoanLimitExceededException()
    {
        var book   = BookFaker.Valid();
        var member = MemberFaker.Valid();

        for (var i = 0; i < LoanLimitExceededException.MaxLoans; i++)
            member.IncrementLoans();

        var cmd = new BorrowBookCommand(book.Id, member.Id);
        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        var act = async () => await _sut.Handle(cmd, default);

        await act.ShouldThrowAsync<LoanLimitExceededException>();
    }
}