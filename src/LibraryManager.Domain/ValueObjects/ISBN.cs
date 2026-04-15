namespace LibraryManager.Domain.ValueObjects;

public sealed class ISBN : IEquatable<ISBN>
{
    public string Value { get; }

    public ISBN(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ISBN cannot be empty.", nameof(value));

        var digits = value.Replace("-", "").Replace(" ", "");

        if (digits.Length != 10 && digits.Length != 13)
            throw new ArgumentException($"'{value}' is not a valid ISBN.", nameof(value));

        Value = value;
    }

    public bool Equals(ISBN? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is ISBN other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static implicit operator string(ISBN isbn) => isbn.Value;
}