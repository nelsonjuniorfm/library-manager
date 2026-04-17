namespace LibraryManager.Application.UseCases.BorrowBook;

public record BorrowBookCommand(Guid BookId, Guid MemberId);