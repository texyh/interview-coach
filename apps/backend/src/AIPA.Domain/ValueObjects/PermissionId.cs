namespace AIPA.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for Permission entities
/// </summary>
public record PermissionId
{
    public Guid Value { get; }

    private PermissionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermissionId cannot be empty", nameof(value));

        Value = value;
    }

    public static PermissionId New() => new(Guid.NewGuid());

    public static PermissionId From(Guid value) => new(value);

    public static PermissionId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid PermissionId format: {value}", nameof(value));

        return new PermissionId(guid);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(PermissionId permissionId) => permissionId.Value;
    public static implicit operator string(PermissionId permissionId) => permissionId.Value.ToString();
}
