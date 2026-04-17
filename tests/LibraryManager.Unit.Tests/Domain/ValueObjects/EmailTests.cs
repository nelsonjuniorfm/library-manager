using LibraryManager.Domain.ValueObjects;
using Shouldly;

namespace LibraryManager.Unit.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("UPPER@DOMAIN.COM")]    // deve normalizar para minúsculas
    [InlineData("name.surname@org.br")]
    public void Constructor_WithValidEmail_CreatesSuccessfully(string value)
    {
        var act = () => new Email(value);

        act.ShouldNotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("semArroba")]
    [InlineData("semponto@dominio")]
    public void Constructor_WithInvalidEmail_ThrowsArgumentException(string value)
    {
        var act = () => new Email(value);

        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_NormalizesToLowercase()
    {
        var email = new Email("USER@EXAMPLE.COM");

        email.Value.ShouldBe("user@example.com");
    }

    [Fact]
    public void TwoEmails_WithSameValueIgnoringCase_AreEqual()
    {
        var a = new Email("user@example.com");
        var b = new Email("USER@EXAMPLE.COM");

        a.ShouldBe(b);
    }
}