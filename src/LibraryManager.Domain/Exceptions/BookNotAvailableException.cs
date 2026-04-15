namespace LibraryManager.Domain.Exceptions;

public sealed class BookNotAvailableException : DomainException
{
    public BookNotAvailableException(string isbn)
        : base($"Book with ISBN '{isbn}' has no available copies.") { }
}