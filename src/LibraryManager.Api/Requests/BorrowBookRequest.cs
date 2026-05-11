namespace LibraryManager.Api.Requests;

public record BorrowBookRequest(Guid BookId, Guid MemberId);