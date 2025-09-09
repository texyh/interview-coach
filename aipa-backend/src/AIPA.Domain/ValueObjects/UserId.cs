namespace AIPA.Domain.ValueObjects;

public record UserId
{
    public Guid Value { get; }

    private UserId(Guid value)
    {
        Value = value;
    }

    public static UserId New() => new(Guid.NewGuid());

    public static UserId From(Guid value) => new(value);

    public static implicit operator Guid(UserId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
