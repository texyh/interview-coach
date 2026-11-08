namespace AIPA.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for UserAuditLog entities
/// </summary>
public record UserAuditLogId
{
    public Guid Value { get; }

    private UserAuditLogId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserAuditLogId cannot be empty", nameof(value));

        Value = value;
    }

    public static UserAuditLogId New() => new(Guid.NewGuid());

    public static UserAuditLogId From(Guid value) => new(value);

    public static UserAuditLogId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid UserAuditLogId format: {value}", nameof(value));

        return new UserAuditLogId(guid);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(UserAuditLogId userAuditLogId) => userAuditLogId.Value;
    public static implicit operator string(UserAuditLogId userAuditLogId) => userAuditLogId.Value.ToString();
}
