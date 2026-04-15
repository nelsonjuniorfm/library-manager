using LibraryManager.Domain.Enums;

namespace LibraryManager.Domain.Entities;

public class Loan
{
    public Guid Id { get; private set; }
    public Guid BookId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateTime BorrowedAt { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? ReturnedAt { get; private set; }
    public LoanStatus Status { get; private set; }

    public Loan(Guid bookId, Guid memberId, DateTime borrowedAt, int loanDurationDays = 14)
    {
        if (loanDurationDays <= 0)
            throw new ArgumentException("Loan duration must be greater than zero.", nameof(loanDurationDays));

        Id = Guid.NewGuid();
        BookId = bookId;
        MemberId = memberId;
        BorrowedAt = borrowedAt;
        DueDate = borrowedAt.AddDays(loanDurationDays);
        Status = LoanStatus.Active;
    }

    public void Return(DateTime returnedAt)
    {
        if (Status != LoanStatus.Active && Status != LoanStatus.Overdue)
            throw new InvalidOperationException("Only active or overdue loans can be returned.");

        ReturnedAt = returnedAt;
        Status = LoanStatus.Returned;
    }

    public bool IsOverdue(DateTime now) => Status == LoanStatus.Active && now > DueDate;

    public void MarkAsOverdue()
    {
        if (Status == LoanStatus.Active)
            Status = LoanStatus.Overdue;
    }
}