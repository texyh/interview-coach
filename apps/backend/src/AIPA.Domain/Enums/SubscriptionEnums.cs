namespace AIPA.Domain.Enums;

/// <summary>
/// Represents subscription tiers in the system
/// </summary>
public enum SubscriptionTier
{
    Free = 1,
    Basic = 2,
    Premium = 3,
    Enterprise = 4
}

/// <summary>
/// Represents subscription status
/// </summary>
public enum SubscriptionStatus
{
    Active = 1,
    Cancelled = 2,
    Expired = 3,
    PastDue = 4,
    Trialing = 5,
    Paused = 6
}

/// <summary>
/// Represents billing cycle intervals
/// </summary>
public enum BillingInterval
{
    Monthly = 1,
    Quarterly = 3,
    Yearly = 12,
    Lifetime = 999
}
