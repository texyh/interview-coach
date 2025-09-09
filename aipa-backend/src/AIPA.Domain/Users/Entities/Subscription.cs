using AIPA.Domain.Common;
using AIPA.Domain.Enums;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Entities;

/// <summary>
/// Represents a user's subscription in the system
/// </summary>
public class Subscription : BaseEntity
{
    public SubscriptionId Id { get; private set; }
    public UserId UserId { get; private set; }
    public SubscriptionTier Tier { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public BillingInterval BillingInterval { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? NextBillingDate { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? ExternalSubscriptionId { get; private set; } // Stripe subscription ID
    public UsageLimits UsageLimits { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;

    private readonly List<SubscriptionUsage> _usageHistory = [];
    public IReadOnlyCollection<SubscriptionUsage> UsageHistory => _usageHistory.AsReadOnly();

    private Subscription()
    {
        Id = null!;
        UserId = null!;
        Currency = null!;
        UsageLimits = null!;
    } // For EF Core

    private Subscription(
        SubscriptionId id,
        UserId userId,
        SubscriptionTier tier,
        BillingInterval billingInterval,
        decimal amount,
        string currency,
        DateTime startDate,
        UsageLimits usageLimits,
        string? externalSubscriptionId = null)
    {
        Id = id;
        UserId = userId;
        Tier = tier;
        Status = SubscriptionStatus.Active;
        BillingInterval = billingInterval;
        Amount = amount;
        Currency = currency;
        StartDate = startDate;
        NextBillingDate = CalculateNextBillingDate(startDate, billingInterval);
        UsageLimits = usageLimits;
        ExternalSubscriptionId = externalSubscriptionId;
    }

    public static Subscription Create(
        UserId userId,
        SubscriptionTier tier,
        BillingInterval billingInterval,
        decimal amount,
        string currency,
        UsageLimits usageLimits,
        string? externalSubscriptionId = null)
    {
        if (amount < 0)
            throw new ArgumentException("Subscription amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        return new Subscription(
            SubscriptionId.New(),
            userId,
            tier,
            billingInterval,
            amount,
            currency.ToUpperInvariant(),
            DateTime.UtcNow,
            usageLimits,
            externalSubscriptionId);
    }

    public void Upgrade(SubscriptionTier newTier, decimal newAmount, UsageLimits newUsageLimits)
    {
        if (newTier <= Tier)
            throw new ArgumentException("Can only upgrade to a higher tier", nameof(newTier));

        Tier = newTier;
        Amount = newAmount;
        UsageLimits = newUsageLimits;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Downgrade(SubscriptionTier newTier, decimal newAmount, UsageLimits newUsageLimits)
    {
        if (newTier >= Tier)
            throw new ArgumentException("Can only downgrade to a lower tier", nameof(newTier));

        Tier = newTier;
        Amount = newAmount;
        UsageLimits = newUsageLimits;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == SubscriptionStatus.Cancelled)
            return;

        Status = SubscriptionStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Renew()
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Can only renew active subscriptions");

        NextBillingDate = CalculateNextBillingDate(DateTime.UtcNow, BillingInterval);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        Status = SubscriptionStatus.Expired;
        EndDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        Status = SubscriptionStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resume()
    {
        if (Status != SubscriptionStatus.Paused)
            throw new InvalidOperationException("Can only resume paused subscriptions");

        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive => Status == SubscriptionStatus.Active && !IsExpired;

    public bool IsExpired => EndDate.HasValue && EndDate.Value <= DateTime.UtcNow;

    public bool CanUseFeature(string featureName) => UsageLimits.CanUseFeature(featureName);

    public void RecordUsage(string featureUsed, int quantity = 1)
    {
        var usage = SubscriptionUsage.Create(Id, featureUsed, quantity);
        _usageHistory.Add(usage);
        UpdatedAt = DateTime.UtcNow;
    }

    private static DateTime? CalculateNextBillingDate(DateTime startDate, BillingInterval interval)
    {
        return interval switch
        {
            BillingInterval.Monthly => startDate.AddMonths(1),
            BillingInterval.Quarterly => startDate.AddMonths(3),
            BillingInterval.Yearly => startDate.AddYears(1),
            BillingInterval.Lifetime => null,
            _ => throw new ArgumentException($"Unknown billing interval: {interval}")
        };
    }
}
