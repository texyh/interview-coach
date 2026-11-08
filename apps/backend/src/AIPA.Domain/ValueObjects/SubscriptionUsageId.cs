namespace AIPA.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for SubscriptionUsage entities
/// </summary>
public record SubscriptionUsageId
{
    public Guid Value { get; }

    private SubscriptionUsageId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SubscriptionUsageId cannot be empty", nameof(value));

        Value = value;
    }

    public static SubscriptionUsageId New() => new(Guid.NewGuid());

    public static SubscriptionUsageId From(Guid value) => new(value);

    public static SubscriptionUsageId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid SubscriptionUsageId format: {value}", nameof(value));

        return new SubscriptionUsageId(guid);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SubscriptionUsageId subscriptionUsageId) => subscriptionUsageId.Value;
    public static implicit operator string(SubscriptionUsageId subscriptionUsageId) => subscriptionUsageId.Value.ToString();
}
