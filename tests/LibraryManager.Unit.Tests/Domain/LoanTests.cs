using LibraryManager.Domain.Enums;
using LibraryManager.Unit.Tests.Helpers;
using Shouldly;

namespace LibraryManager.Unit.Tests.Domain;

public class LoanTests
{
    // --- Constructor ---

    [Fact]
    public void Constructor_SetsDueDateCorrectly()
    {
        var borrowedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var loan = new LibraryManager.Domain.Entities.Loan(
            Guid.NewGuid(), Guid.NewGuid(), borrowedAt, loanDurationDays: 14);

        loan.DueDate.ShouldBe(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Constructor_SetsStatusAsActive()
    {
        var loan = LoanFaker.Active();

        loan.Status.ShouldBe(LoanStatus.Active);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidDuration_ThrowsArgumentException(int days)
    {
        var act = () => new LibraryManager.Domain.Entities.Loan(
            Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, days);

        act.ShouldThrow<ArgumentException>()
           .ParamName.ShouldBe("loanDurationDays");
    }

    // --- Return ---

    [Fact]
    public void Return_WhenActive_SetsStatusToReturned()
    {
        var loan = LoanFaker.Active();

        loan.Return(DateTime.UtcNow);

        loan.Status.ShouldBe(LoanStatus.Returned);
    }

    [Fact]
    public void Return_WhenActive_SetsReturnedAt()
    {
        var loan = LoanFaker.Active();
        var returnedAt = DateTime.UtcNow;

        loan.Return(returnedAt);

        loan.ReturnedAt.ShouldBe(returnedAt);
    }

    [Fact]
    public void Return_WhenAlreadyReturned_ThrowsInvalidOperationException()
    {
        var loan = LoanFaker.Active();
        loan.Return(DateTime.UtcNow);

        var act = () => loan.Return(DateTime.UtcNow);

        act.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Return_WhenOverdue_SetsStatusToReturned()
    {
        var loan = LoanFaker.Overdue();
        loan.MarkAsOverdue();

        loan.Return(DateTime.UtcNow);

        loan.Status.ShouldBe(LoanStatus.Returned);
    }

    // --- IsOverdue ---

    [Fact]
    public void IsOverdue_WhenPastDueDate_ReturnsTrue()
    {
        var loan = LoanFaker.Overdue(); // borrowedAt -20 dias, duration 14

        loan.IsOverdue(DateTime.UtcNow).ShouldBeTrue();
    }

    [Fact]
    public void IsOverdue_WhenBeforeDueDate_ReturnsFalse()
    {
        var loan = LoanFaker.Active(); // borrowedAt -3 dias, duration 14

        loan.IsOverdue(DateTime.UtcNow).ShouldBeFalse();
    }

    [Fact]
    public void IsOverdue_WhenAlreadyReturned_ReturnsFalse()
    {
        var loan = LoanFaker.Overdue();
        loan.Return(DateTime.UtcNow);

        loan.IsOverdue(DateTime.UtcNow).ShouldBeFalse();
    }

    // --- MarkAsOverdue ---

    [Fact]
    public void MarkAsOverdue_WhenActive_ChangesStatusToOverdue()
    {
        var loan = LoanFaker.Overdue();

        loan.MarkAsOverdue();

        loan.Status.ShouldBe(LoanStatus.Overdue);
    }

    [Fact]
    public void MarkAsOverdue_WhenAlreadyReturned_DoesNotChangeStatus()
    {
        var loan = LoanFaker.Active();
        loan.Return(DateTime.UtcNow);

        loan.MarkAsOverdue();

        loan.Status.ShouldBe(LoanStatus.Returned);
    }
}