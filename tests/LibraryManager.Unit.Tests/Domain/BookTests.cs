using LibraryManager.Domain.Exceptions;
using LibraryManager.Unit.Tests.Helpers;
using Shouldly;

namespace LibraryManager.Unit.Tests.Domain;

public class BookTests
{
    // --- Reserve ---

    [Fact]
    public void Reserve_WhenCopiesAvailable_DecrementsAvailableCopies()
    {
        var book = BookFaker.Valid(2);

        book.Reserve();

        book.AvailableCopies.ShouldBe(1);
    }

    [Fact]
    public void Reserve_WhenNoCopiesLeft_ThrowsBookNotAvailableException()
    {
        var book = BookFaker.Valid(1);
        book.Reserve(); // esgota

        var act = () => book.Reserve();

        act.ShouldThrow<BookNotAvailableException>()
           .Message.ShouldContain(book.ISBN.Value);
    }

    [Fact]
    public void Reserve_MultipleReservations_KeepsAccurateCount()
    {
        var book = BookFaker.Valid(3);

        book.Reserve();
        book.Reserve();

        book.AvailableCopies.ShouldBe(1);
    }

    // --- Release ---

    [Fact]
    public void Release_AfterReservation_IncrementsAvailableCopies()
    {
        var book = BookFaker.Valid(2);
        book.Reserve();

        book.Release();

        book.AvailableCopies.ShouldBe(2);
    }

    [Fact]
    public void Release_WithNoReservations_ThrowsInvalidOperationException()
    {
        var book = BookFaker.Valid(2);

        var act = () => book.Release();

        act.ShouldThrow<InvalidOperationException>();
    }

    // --- Constructor ---

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithInvalidTotalCopies_ThrowsArgumentException(int copies)
    {
        var act = () => BookFaker.Valid(copies);

        act.ShouldThrow<ArgumentException>()
           .ParamName.ShouldBe("totalCopies");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException(string title)
    {
        var act = () => new LibraryManager.Domain.Entities.Book(
            new LibraryManager.Domain.ValueObjects.ISBN("978-3-16-148410-0"),
            title,
            "Autor Válido",
            1);

        act.ShouldThrow<ArgumentException>()
           .ParamName.ShouldBe("title");
    }
}