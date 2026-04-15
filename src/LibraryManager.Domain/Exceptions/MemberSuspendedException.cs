namespace LibraryManager.Domain.Exceptions;

public sealed class MemberSuspendedException : DomainException
{
    public MemberSuspendedException(Guid memberId)
        : base($"Member '{memberId}' is suspended and cannot borrow books.") { }
}