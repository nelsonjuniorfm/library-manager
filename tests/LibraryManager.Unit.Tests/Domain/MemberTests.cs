using LibraryManager.Domain.Enums;
using LibraryManager.Domain.Exceptions;
using LibraryManager.Unit.Tests.Helpers;
using Shouldly;

namespace LibraryManager.Unit.Tests.Domain;

public class MemberTests
{
    // --- CanBorrow ---

    [Fact]
    public void CanBorrow_WhenActiveAndBelowLimit_DoesNotThrow()
    {
        var member = MemberFaker.Valid();

        var act = () => member.CanBorrow();

        act.ShouldNotThrow();
    }

    [Fact]
    public void CanBorrow_WhenSuspended_ThrowsMemberSuspendedException()
    {
        var member = MemberFaker.Valid();
        member.Suspend();

        var act = () => member.CanBorrow();

        act.ShouldThrow<MemberSuspendedException>()
           .Message.ShouldContain(member.Id.ToString());
    }

    [Fact]
    public void CanBorrow_WhenAtLoanLimit_ThrowsLoanLimitExceededException()
    {
        var member = MemberFaker.Valid();

        // Simula 3 empréstimos ativos
        for (var i = 0; i < LoanLimitExceededException.MaxLoans; i++)
            member.IncrementLoans();

        var act = () => member.CanBorrow();

        act.ShouldThrow<LoanLimitExceededException>()
           .Message.ShouldContain(member.Id.ToString());
    }

    // --- IncrementLoans / DecrementLoans ---

    [Fact]
    public void IncrementLoans_IncreasesActiveLoansCount()
    {
        var member = MemberFaker.Valid();

        member.IncrementLoans();
        member.IncrementLoans();

        member.ActiveLoans.ShouldBe(2);
    }

    [Fact]
    public void DecrementLoans_DecreasesActiveLoansCount()
    {
        var member = MemberFaker.Valid();
        member.IncrementLoans();
        member.IncrementLoans();

        member.DecrementLoans();

        member.ActiveLoans.ShouldBe(1);
    }

    [Fact]
    public void DecrementLoans_WhenZero_DoesNotGoBelowZero()
    {
        var member = MemberFaker.Valid();

        member.DecrementLoans();

        member.ActiveLoans.ShouldBe(0);
    }

    // --- Suspend / Activate ---

    [Fact]
    public void Suspend_ChangesStatusToSuspended()
    {
        var member = MemberFaker.Valid();

        member.Suspend();

        member.Status.ShouldBe(MemberStatus.Suspended);
    }

    [Fact]
    public void Activate_ChangesStatusToActive()
    {
        var member = MemberFaker.Valid();
        member.Suspend();

        member.Activate();

        member.Status.ShouldBe(MemberStatus.Active);
    }
}