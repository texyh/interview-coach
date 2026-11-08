namespace AIPA.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for UserRole entities
/// </summary>
public record UserRoleId
{
    public Guid Value { get; }

    private UserRoleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserRoleId cannot be empty", nameof(value));

        Value = value;
    }

    public static UserRoleId New() => new(Guid.NewGuid());

    public static UserRoleId From(Guid value) => new(value);

    public static UserRoleId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid UserRoleId format: {value}", nameof(value));

        return new UserRoleId(guid);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(UserRoleId userRoleId) => userRoleId.Value;
    public static implicit operator string(UserRoleId userRoleId) => userRoleId.Value.ToString();
}
