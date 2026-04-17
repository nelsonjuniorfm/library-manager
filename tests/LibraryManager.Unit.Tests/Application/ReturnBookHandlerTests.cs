using LibraryManager.Domain.Enums;
using LibraryManager.Domain.Interfaces;
using LibraryManager.Unit.Tests.Helpers;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Shouldly;
using LibraryManager.Application.UseCases.ReturnBook;

namespace LibraryManager.Unit.Tests.Application;

public class ReturnBookHandlerTests
{
    private readonly IBookRepository   _books   = Substitute.For<IBookRepository>();
    private readonly IMemberRepository _members = Substitute.For<IMemberRepository>();
    private readonly ILoanRepository   _loans   = Substitute.For<ILoanRepository>();
    private readonly ReturnBookHandler _sut;

    public ReturnBookHandlerTests()
        => _sut = new ReturnBookHandler(_books, _members, _loans);

    [Fact]
    public async Task Handle_WhenValid_SetsLoanStatusToReturned()
    {
        var book   = BookFaker.Valid();
        var member = MemberFaker.Valid();
        var loan   = LoanFaker.Active(book.Id, member.Id);
        member.IncrementLoans();

        _loans.GetByIdAsync(loan.Id, default).Returns(loan);
        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        await _sut.Handle(new ReturnBookCommand(loan.Id), default);

        loan.Status.ShouldBe(LoanStatus.Returned);
    }

    [Fact]
    public async Task Handle_WhenValid_ReleasesBookCopy()
    {
        var book   = BookFaker.Valid(totalCopies: 2);
        var member = MemberFaker.Valid();
        var loan   = LoanFaker.Active(book.Id, member.Id);
        book.Reserve();
        member.IncrementLoans();

        _loans.GetByIdAsync(loan.Id, default).Returns(loan);
        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        await _sut.Handle(new ReturnBookCommand(loan.Id), default);

        book.AvailableCopies.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenValid_DecrementsActiveLoanOnMember()
    {
        var book   = BookFaker.Valid();
        var member = MemberFaker.Valid();
        var loan   = LoanFaker.Active(book.Id, member.Id);
        member.IncrementLoans();

        _loans.GetByIdAsync(loan.Id, default).Returns(loan);
        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        await _sut.Handle(new ReturnBookCommand(loan.Id), default);

        member.ActiveLoans.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesLoanAndBookAndMember()
    {
        var book   = BookFaker.Valid();
        var member = MemberFaker.Valid();
        var loan   = LoanFaker.Active(book.Id, member.Id);
        member.IncrementLoans();

        _loans.GetByIdAsync(loan.Id, default).Returns(loan);
        _books.GetByIdAsync(book.Id, default).Returns(book);
        _members.GetByIdAsync(member.Id, default).Returns(member);

        await _sut.Handle(new ReturnBookCommand(loan.Id), default);

        await _loans.Received(1).UpdateAsync(loan, default);
        await _books.Received(1).UpdateAsync(book, default);
        await _members.Received(1).UpdateAsync(member, default);
    }

    [Fact]
    public async Task Handle_WhenLoanNotFound_ThrowsKeyNotFoundException()
    {
        _loans.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var act = async () =>
            await _sut.Handle(new ReturnBookCommand(Guid.NewGuid()), default);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }
}