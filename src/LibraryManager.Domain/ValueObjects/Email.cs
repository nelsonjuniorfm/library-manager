namespace LibraryManager.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.", nameof(value));

        if (!value.Contains('@') || !value.Contains('.'))
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));

        Value = value.ToLowerInvariant();
    }

    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Email other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}