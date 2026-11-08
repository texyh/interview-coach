namespace AIPA.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for Subscription entities
/// </summary>
public record SubscriptionId
{
    public Guid Value { get; }

    private SubscriptionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SubscriptionId cannot be empty", nameof(value));

        Value = value;
    }

    public static SubscriptionId New() => new(Guid.NewGuid());

    public static SubscriptionId From(Guid value) => new(value);

    public static SubscriptionId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid SubscriptionId format: {value}", nameof(value));

        return new SubscriptionId(guid);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SubscriptionId subscriptionId) => subscriptionId.Value;
    public static implicit operator string(SubscriptionId subscriptionId) => subscriptionId.Value.ToString();
}
