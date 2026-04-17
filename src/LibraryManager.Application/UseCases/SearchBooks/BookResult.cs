namespace LibraryManager.Application.UseCases.SearchBooks;

// DTO de saída — não expõe a entidade diretamente para fora da Application
public record BookResult(
    Guid Id,
    string ISBN,
    string Title,
    string Author,
    int AvailableCopies,
    int TotalCopies);