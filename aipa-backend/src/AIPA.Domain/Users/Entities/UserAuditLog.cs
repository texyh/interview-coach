using AIPA.Domain.Common;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Entities;

/// <summary>
/// Audit log for tracking user operations
/// </summary>
public class UserAuditLog : BaseEntity
{
    public UserAuditLogId Id { get; private set; }
    public UserId UserId { get; private set; }
    public string Action { get; private set; }
    public string EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public string? OldValues { get; private set; } // JSON
    public string? NewValues { get; private set; } // JSON
    public DateTime Timestamp { get; private set; }
    public string? PerformedBy { get; private set; } // User ID or system that performed the action
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public string? AdditionalInfo { get; private set; } // JSON for extra metadata

    // Navigation properties
    public User User { get; private set; } = null!;

    private UserAuditLog()
    {
        Id = null!;
        UserId = null!;
        Action = null!;
        EntityType = null!;
        IpAddress = null!;
        UserAgent = null!;
    } // For EF Core

    private UserAuditLog(
        UserAuditLogId id,
        UserId userId,
        string action,
        string entityType,
        string? entityId,
        string? oldValues,
        string? newValues,
        DateTime timestamp,
        string? performedBy,
        string ipAddress,
        string userAgent,
        string? additionalInfo)
    {
        Id = id;
        UserId = userId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        OldValues = oldValues;
        NewValues = newValues;
        Timestamp = timestamp;
        PerformedBy = performedBy;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        AdditionalInfo = additionalInfo;
    }

    public static UserAuditLog Create(
        UserId userId,
        string action,
        string entityType,
        string? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string? performedBy = null,
        string ipAddress = "unknown",
        string userAgent = "unknown",
        string? additionalInfo = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty", nameof(action));

        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be empty", nameof(entityType));

        return new UserAuditLog(
            UserAuditLogId.New(),
            userId,
            action.Trim(),
            entityType.Trim(),
            entityId?.Trim(),
            oldValues,
            newValues,
            DateTime.UtcNow,
            performedBy?.Trim(),
            ipAddress.Trim(),
            userAgent.Trim(),
            additionalInfo);
    }

    // Common audit log creation methods for convenience
    public static UserAuditLog UserCreated(UserId userId, string? performedBy = null, string ipAddress = "unknown", string userAgent = "unknown") =>
        Create(userId, "Created", "User", userId.ToString(), null, null, performedBy, ipAddress, userAgent);

    public static UserAuditLog UserUpdated(UserId userId, string? oldValues, string? newValues, string? performedBy = null, string ipAddress = "unknown", string userAgent = "unknown") =>
        Create(userId, "Updated", "User", userId.ToString(), oldValues, newValues, performedBy, ipAddress, userAgent);

    public static UserAuditLog UserDeleted(UserId userId, string? performedBy = null, string ipAddress = "unknown", string userAgent = "unknown") =>
        Create(userId, "Deleted", "User", userId.ToString(), null, null, performedBy, ipAddress, userAgent);

    public static UserAuditLog ProfileUpdated(UserId userId, string? oldValues, string? newValues, string? performedBy = null, string ipAddress = "unknown", string userAgent = "unknown") =>
        Create(userId, "Updated", "UserProfile", userId.ToString(), oldValues, newValues, performedBy, ipAddress, userAgent);

    public static UserAuditLog RoleAssigned(UserId userId, string roleId, string? performedBy = null, string ipAddress = "unknown", string userAgent = "unknown") =>
        Create(userId, "RoleAssigned", "UserRole", roleId, null, $"{{\"RoleId\":\"{roleId}\"}}", performedBy, ipAddress, userAgent);

    public static UserAuditLog RoleRemoved(UserId userId, string roleId, string? performedBy = null, string ipAddress = "unknown", string userAgent = "unknown") =>
        Create(userId, "RoleRemoved", "UserRole", roleId, $"{{\"RoleId\":\"{roleId}\"}}", null, performedBy, ipAddress, userAgent);

    public static UserAuditLog SubscriptionChanged(UserId userId, string subscriptionId, string? oldValues, string? newValues, string? performedBy = null, string ipAddress = "unknown", string userAgent = "unknown") =>
        Create(userId, "SubscriptionChanged", "Subscription", subscriptionId, oldValues, newValues, performedBy, ipAddress, userAgent);

    public static UserAuditLog LoginAttempt(UserId userId, bool successful, string ipAddress = "unknown", string userAgent = "unknown") =>
        Create(userId, successful ? "LoginSuccess" : "LoginFailed", "Authentication", null, null, $"{{\"Success\":{successful.ToString().ToLowerInvariant()}}}", null, ipAddress, userAgent);
}
