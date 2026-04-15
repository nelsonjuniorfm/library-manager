using LibraryManager.Domain.Enums;
using LibraryManager.Domain.Exceptions;
using LibraryManager.Domain.ValueObjects;

namespace LibraryManager.Domain.Entities;

public class Member
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public int ActiveLoans { get; private set; }
    public MemberStatus Status { get; private set; }

    public Member(string name, Email email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        ActiveLoans = 0;
        Status = MemberStatus.Active;
    }

    public void CanBorrow()
    {
        if (Status == MemberStatus.Suspended)
            throw new MemberSuspendedException(Id);

        if (ActiveLoans >= LoanLimitExceededException.MaxLoans)
            throw new LoanLimitExceededException(Id);
    }

    public void IncrementLoans() => ActiveLoans++;

    public void DecrementLoans()
    {
        if (ActiveLoans > 0)
            ActiveLoans--;
    }

    public void Suspend() => Status = MemberStatus.Suspended;

    public void Activate() => Status = MemberStatus.Active;
}