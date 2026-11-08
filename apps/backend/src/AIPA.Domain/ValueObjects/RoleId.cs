namespace AIPA.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for Role entities
/// </summary>
public record RoleId
{
    public Guid Value { get; }

    private RoleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RoleId cannot be empty", nameof(value));

        Value = value;
    }

    public static RoleId New() => new(Guid.NewGuid());

    public static RoleId From(Guid value) => new(value);

    public static RoleId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid RoleId format: {value}", nameof(value));

        return new RoleId(guid);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(RoleId roleId) => roleId.Value;
    public static implicit operator string(RoleId roleId) => roleId.Value.ToString();
}
