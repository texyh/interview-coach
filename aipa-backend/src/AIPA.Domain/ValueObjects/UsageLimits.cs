using AIPA.Domain.Enums;

namespace AIPA.Domain.ValueObjects;

/// <summary>
/// Represents usage limits for different subscription tiers
/// </summary>
public record UsageLimits
{
    public int MonthlyInterviewSessions { get; }
    public int MonthlyAIAnalysis { get; }
    public int StorageInMB { get; }
    public bool CanAccessPremiumQuestions { get; }
    public bool CanExportData { get; }
    public bool HasPrioritySupport { get; }
    public int MaxConcurrentSessions { get; }

    private UsageLimits(
        int monthlyInterviewSessions,
        int monthlyAIAnalysis,
        int storageInMB,
        bool canAccessPremiumQuestions,
        bool canExportData,
        bool hasPrioritySupport,
        int maxConcurrentSessions)
    {
        MonthlyInterviewSessions = monthlyInterviewSessions;
        MonthlyAIAnalysis = monthlyAIAnalysis;
        StorageInMB = storageInMB;
        CanAccessPremiumQuestions = canAccessPremiumQuestions;
        CanExportData = canExportData;
        HasPrioritySupport = hasPrioritySupport;
        MaxConcurrentSessions = maxConcurrentSessions;
    }

    public static UsageLimits ForTier(SubscriptionTier tier)
    {
        return tier switch
        {
            SubscriptionTier.Free => new UsageLimits(
                monthlyInterviewSessions: 5,
                monthlyAIAnalysis: 3,
                storageInMB: 100,
                canAccessPremiumQuestions: false,
                canExportData: false,
                hasPrioritySupport: false,
                maxConcurrentSessions: 1),

            SubscriptionTier.Basic => new UsageLimits(
                monthlyInterviewSessions: 25,
                monthlyAIAnalysis: 15,
                storageInMB: 500,
                canAccessPremiumQuestions: false,
                canExportData: true,
                hasPrioritySupport: false,
                maxConcurrentSessions: 2),

            SubscriptionTier.Premium => new UsageLimits(
                monthlyInterviewSessions: 100,
                monthlyAIAnalysis: 50,
                storageInMB: 2000,
                canAccessPremiumQuestions: true,
                canExportData: true,
                hasPrioritySupport: true,
                maxConcurrentSessions: 5),

            SubscriptionTier.Enterprise => new UsageLimits(
                monthlyInterviewSessions: -1, // Unlimited
                monthlyAIAnalysis: -1, // Unlimited
                storageInMB: -1, // Unlimited
                canAccessPremiumQuestions: true,
                canExportData: true,
                hasPrioritySupport: true,
                maxConcurrentSessions: -1), // Unlimited

            _ => throw new ArgumentException($"Unknown subscription tier: {tier}")
        };
    }

    public static UsageLimits Create(
        int monthlyInterviewSessions,
        int monthlyAIAnalysis,
        int storageInMB,
        bool canAccessPremiumQuestions = false,
        bool canExportData = false,
        bool hasPrioritySupport = false,
        int maxConcurrentSessions = 1)
    {
        return new UsageLimits(
            monthlyInterviewSessions,
            monthlyAIAnalysis,
            storageInMB,
            canAccessPremiumQuestions,
            canExportData,
            hasPrioritySupport,
            maxConcurrentSessions);
    }

    public bool CanUseFeature(string featureName)
    {
        return featureName.ToLowerInvariant() switch
        {
            "premiumquestions" => CanAccessPremiumQuestions,
            "exportdata" => CanExportData,
            "prioritysupport" => HasPrioritySupport,
            _ => true // Default allow for unknown features
        };
    }

    public bool IsUnlimited(string limitType)
    {
        return limitType.ToLowerInvariant() switch
        {
            "interviews" => MonthlyInterviewSessions == -1,
            "analysis" => MonthlyAIAnalysis == -1,
            "storage" => StorageInMB == -1,
            "sessions" => MaxConcurrentSessions == -1,
            _ => false
        };
    }
}
