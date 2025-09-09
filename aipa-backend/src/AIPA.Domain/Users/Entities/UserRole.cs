using AIPA.Domain.Common;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Entities;

/// <summary>
/// Junction entity for User-Role many-to-many relationship
/// </summary>
public class UserRole : BaseEntity
{
    public UserRoleId Id { get; private set; }
    public UserId UserId { get; private set; }
    public RoleId RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public string? AssignedBy { get; private set; }
    public string? Reason { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole()
    {
        Id = null!;
        UserId = null!;
        RoleId = null!;
    } // For EF Core

    private UserRole(
        UserRoleId id,
        UserId userId,
        RoleId roleId,
        DateTime assignedAt,
        DateTime? expiresAt,
        string? assignedBy,
        string? reason)
    {
        Id = id;
        UserId = userId;
        RoleId = roleId;
        AssignedAt = assignedAt;
        ExpiresAt = expiresAt;
        IsActive = true;
        AssignedBy = assignedBy;
        Reason = reason;
    }

    public static UserRole Create(
        UserId userId,
        RoleId roleId,
        DateTime? expiresAt = null,
        string? assignedBy = null,
        string? reason = null)
    {
        return new UserRole(
            UserRoleId.New(),
            userId,
            roleId,
            DateTime.UtcNow,
            expiresAt,
            assignedBy,
            reason);
    }

    public void Deactivate(string? reason = null)
    {
        IsActive = false;
        Reason = reason ?? Reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Extend(DateTime newExpirationDate, string? reason = null)
    {
        if (newExpirationDate <= DateTime.UtcNow)
            throw new ArgumentException("New expiration date must be in the future", nameof(newExpirationDate));

        ExpiresAt = newExpirationDate;
        Reason = reason ?? Reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    public bool IsValidAndActive => IsActive && !IsExpired;
}
