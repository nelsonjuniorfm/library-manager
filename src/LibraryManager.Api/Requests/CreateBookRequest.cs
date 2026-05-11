namespace LibraryManager.Api.Requests;

public record CreateBookRequest(
    string ISBN,
    string Title,
    string Author,
    int    TotalCopies);