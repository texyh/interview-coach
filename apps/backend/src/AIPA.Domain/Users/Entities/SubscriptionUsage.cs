using AIPA.Domain.Common;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Entities;

/// <summary>
/// Tracks usage of subscription features
/// </summary>
public class SubscriptionUsage : BaseEntity
{
    public SubscriptionUsageId Id { get; private set; }
    public SubscriptionId SubscriptionId { get; private set; }
    public string FeatureUsed { get; private set; }
    public int Quantity { get; private set; }
    public DateTime UsedAt { get; private set; }
    public string? Metadata { get; private set; } // JSON for additional data

    // Navigation properties
    public Subscription Subscription { get; private set; } = null!;

    private SubscriptionUsage()
    {
        Id = null!;
        SubscriptionId = null!;
        FeatureUsed = null!;
    } // For EF Core

    private SubscriptionUsage(
        SubscriptionUsageId id,
        SubscriptionId subscriptionId,
        string featureUsed,
        int quantity,
        DateTime usedAt,
        string? metadata = null)
    {
        Id = id;
        SubscriptionId = subscriptionId;
        FeatureUsed = featureUsed;
        Quantity = quantity;
        UsedAt = usedAt;
        Metadata = metadata;
    }

    public static SubscriptionUsage Create(
        SubscriptionId subscriptionId,
        string featureUsed,
        int quantity = 1,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(featureUsed))
            throw new ArgumentException("Feature used cannot be empty", nameof(featureUsed));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        return new SubscriptionUsage(
            SubscriptionUsageId.New(),
            subscriptionId,
            featureUsed.Trim(),
            quantity,
            DateTime.UtcNow,
            metadata);
    }
}
