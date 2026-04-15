namespace LibraryManager.Domain.Exceptions;

public sealed class LoanLimitExceededException : DomainException
{
    public const int MaxLoans = 3;

    public LoanLimitExceededException(Guid memberId)
        : base($"Member '{memberId}' has reached the limit of {MaxLoans} active loans.") { }
}