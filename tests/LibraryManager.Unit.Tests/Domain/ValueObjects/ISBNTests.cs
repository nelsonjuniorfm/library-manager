using LibraryManager.Domain.ValueObjects;
using Shouldly;

namespace LibraryManager.Unit.Tests.Domain.ValueObjects;

public class ISBNTests
{
    [Theory]
    [InlineData("978-3-16-148410-0")]   // ISBN-13 com hífens
    [InlineData("9783161484100")]       // ISBN-13 sem hífens
    [InlineData("0-306-40615-2")]       // ISBN-10 com hífens
    public void Constructor_WithValidISBN_CreatesSuccessfully(string value)
    {
        var act = () => new ISBN(value);

        act.ShouldNotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("99999999999999")]  // 14 dígitos
    public void Constructor_WithInvalidISBN_ThrowsArgumentException(string value)
    {
        var act = () => new ISBN(value);

        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void TwoISBNs_WithSameValue_AreEqual()
    {
        var a = new ISBN("978-3-16-148410-0");
        var b = new ISBN("978-3-16-148410-0");

        a.ShouldBe(b);
    }

    [Fact]
    public void TwoISBNs_WithDifferentValues_AreNotEqual()
    {
        var a = new ISBN("978-3-16-148410-0");
        var b = new ISBN("9780201379624");

        a.ShouldNotBe(b);
    }
}