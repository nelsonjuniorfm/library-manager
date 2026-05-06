using LibraryManager.Domain.Exceptions;
using LibraryManager.Domain.ValueObjects;

namespace LibraryManager.Domain.Entities;

public class Book
{
    public Guid Id { get; private set; }
    public ISBN ISBN { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public int TotalCopies { get; private set; }
    public int AvailableCopies { get; private set; }

    public Book(ISBN isbn, string title, string author, int totalCopies)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be empty.", nameof(author));

        if (totalCopies <= 0)
            throw new ArgumentException("Total copies must be greater than zero.", nameof(totalCopies));

        Id = Guid.NewGuid();
        ISBN = isbn;
        Title = title;
        Author = author;
        TotalCopies = totalCopies;
        AvailableCopies = totalCopies;
    }

    private Book() { }

    public static Book Reconstitute(
        Guid id, ISBN isbn, string title, string author,
        int totalCopies, int availableCopies) => new()
        {
            Id = id,
            ISBN = isbn,
            Title = title,
            Author = author,
            TotalCopies = totalCopies,
            AvailableCopies = availableCopies
        };

    public void Reserve()
    {
        if (AvailableCopies <= 0)
            throw new BookNotAvailableException(ISBN.Value);

        AvailableCopies--;
    }

    public void Release()
    {
        if (AvailableCopies >= TotalCopies)
            throw new InvalidOperationException("Cannot release a copy that was not reserved.");

        AvailableCopies++;
    }
}