using AIPA.Domain.Common;
using AIPA.Domain.Common.Exceptions;
using AIPA.Domain.Enums;
using AIPA.Domain.Interviews.Entities;
using AIPA.Domain.Users.Events;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Entities;

public class User : BaseEntity
{
    public UserId Id { get; private set; }
    public Email Email { get; private set; }
    public FullName FullName { get; private set; }
    public UserProfile Profile { get; private set; }
    public UserPreferences Preferences { get; private set; }
    public AccountStatus Status { get; private set; }

    // Authentication properties
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    // Email verification properties
    public bool IsEmailVerified { get; private set; } = false;
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }

    // Password reset properties
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    // Convenience properties for authentication
    public string EmailAddress => Email.Value;
    public string FirstName => FullName.FirstName;
    public string LastName => FullName.LastName;
    public AccountStatus AccountStatus => Status;

    private readonly List<InterviewSession> _interviewSessions = [];
    public IReadOnlyCollection<InterviewSession> InterviewSessions => _interviewSessions.AsReadOnly();

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<Subscription> _subscriptions = [];
    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();

    private readonly List<UserAuditLog> _auditLogs = [];
    public IReadOnlyCollection<UserAuditLog> AuditLogs => _auditLogs.AsReadOnly();

    private User()
    {
        Id = null!;
        Email = null!;
        FullName = null!;
        Profile = null!;
        Preferences = null!;
    } // For EF Core

    private User(UserId id, Email email, FullName fullName, UserProfile profile, UserPreferences preferences, AccountStatus status)
    {
        Id = id;
        Email = email;
        FullName = fullName;
        Profile = profile;
        Preferences = preferences;
        Status = status;
    }

    public static User Create(Email email, FullName fullName)
    {
        var user = new User(
            UserId.New(),
            email,
            fullName,
            UserProfile.CreateDefault(),
            UserPreferences.CreateDefault(),
            AccountStatus.Active);

        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email));
        return user;
    }

    public void UpdateProfile(UserProfile profile)
    {
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserProfileUpdatedEvent(Id, Profile));
    }

    public void UpdatePreferences(UserPreferences preferences)
    {
        Preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserPreferencesUpdatedEvent(Id, Preferences));
    }

    public void UpdateEmail(Email newEmail)
    {
        if (newEmail == Email) return;

        Email = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFullName(FullName newFullName)
    {
        if (newFullName == FullName) return;

        FullName = newFullName ?? throw new ArgumentNullException(nameof(newFullName));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleValidationException("Suspension reason cannot be empty");

        Status = AccountStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = AccountStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = AccountStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(AccountStatus status)
    {
        if (Status == status) return;

        Status = status;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserStatusChangedEvent(Id, status));
    }

    public void MarkAsDeleted()
    {
        Status = AccountStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserDeletedEvent(Id));
    }

    public bool CanScheduleInterview()
    {
        return Status == AccountStatus.Active;
    }

    // Role Management
    public void AssignRole(Role role, DateTime? expiresAt = null, string? assignedBy = null, string? reason = null)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (HasRole(role.Name))
            return; // Already has role

        var userRole = UserRole.Create(Id, role.Id, expiresAt, assignedBy, reason);
        _userRoles.Add(userRole);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserRoleAssignedEvent(Id, role.Id, role.Name));
    }

    public void RemoveRole(Role role, string? reason = null)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == role.Id && ur.IsValidAndActive);
        if (userRole == null)
            return; // Doesn't have role

        userRole.Deactivate(reason);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserRoleRemovedEvent(Id, role.Id, role.Name));
    }

    public bool HasRole(string roleName)
    {
        return _userRoles.Any(ur => ur.IsValidAndActive && ur.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasPermission(string permissionName)
    {
        return _userRoles
            .Where(ur => ur.IsValidAndActive)
            .Any(ur => ur.Role.HasPermission(permissionName));
    }

    public IEnumerable<string> GetActiveRoles()
    {
        return _userRoles
            .Where(ur => ur.IsValidAndActive)
            .Select(ur => ur.Role.Name);
    }

    public IEnumerable<string> GetActivePermissions()
    {
        return _userRoles
            .Where(ur => ur.IsValidAndActive)
            .SelectMany(ur => ur.Role.Permissions)
            .Where(p => p.IsActive)
            .Select(p => p.Name)
            .Distinct();
    }

    // Subscription Management
    public void AddSubscription(Subscription subscription)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));

        // Deactivate existing active subscriptions
        foreach (var existingSubscription in _subscriptions.Where(s => s.IsActive))
        {
            existingSubscription.Cancel("Replaced by new subscription");
        }

        _subscriptions.Add(subscription);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserSubscriptionChangedEvent(Id, subscription.Id, subscription.Tier));
    }

    public Subscription? GetActiveSubscription()
    {
        return _subscriptions.FirstOrDefault(s => s.IsActive);
    }

    public SubscriptionTier GetCurrentTier()
    {
        var activeSubscription = GetActiveSubscription();
        return activeSubscription?.Tier ?? SubscriptionTier.Free;
    }

    public bool CanUseFeature(string featureName)
    {
        var activeSubscription = GetActiveSubscription();
        return activeSubscription?.CanUseFeature(featureName) ?? false;
    }

    public void RecordUsage(string featureUsed, int quantity = 1)
    {
        var activeSubscription = GetActiveSubscription();
        activeSubscription?.RecordUsage(featureUsed, quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    // Audit Logging
    public void AddAuditLog(UserAuditLog auditLog)
    {
        if (auditLog == null)
            throw new ArgumentNullException(nameof(auditLog));

        _auditLogs.Add(auditLog);
    }

    // Authentication methods
    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;

        // Lock account after 5 failed attempts for 30 minutes
        if (FailedLoginAttempts >= 5)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(30);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsAccountLocked()
    {
        return LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;
    }

    public void UnlockAccount()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    // Email verification methods
    public void GenerateEmailVerificationToken()
    {
        EmailVerificationToken = Guid.NewGuid().ToString();
        EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyEmail(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Verification token cannot be null or empty", nameof(token));

        if (EmailVerificationToken != token)
            throw new BusinessRuleValidationException("Invalid verification token");

        if (EmailVerificationTokenExpiresAt == null || EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            throw new BusinessRuleValidationException("Verification token has expired");

        IsEmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserEmailVerifiedEvent(Id, Email));
    }

    public bool IsEmailVerificationTokenValid(string token)
    {
        return !string.IsNullOrWhiteSpace(EmailVerificationToken) &&
               EmailVerificationToken == token &&
               EmailVerificationTokenExpiresAt.HasValue &&
               EmailVerificationTokenExpiresAt > DateTime.UtcNow;
    }

    // Password reset methods
    public void GeneratePasswordResetToken()
    {
        PasswordResetToken = Guid.NewGuid().ToString();
        PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetPassword(string token, string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Reset token cannot be null or empty", nameof(token));

        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(newPasswordHash));

        if (PasswordResetToken != token)
            throw new BusinessRuleValidationException("Invalid reset token");

        if (PasswordResetTokenExpiresAt == null || PasswordResetTokenExpiresAt < DateTime.UtcNow)
            throw new BusinessRuleValidationException("Reset token has expired");

        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        FailedLoginAttempts = 0; // Reset failed attempts on successful password reset
        LockedUntil = null; // Unlock account if it was locked
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserPasswordResetEvent(Id, Email));
    }

    public bool IsPasswordResetTokenValid(string token)
    {
        return !string.IsNullOrWhiteSpace(PasswordResetToken) &&
               PasswordResetToken == token &&
               PasswordResetTokenExpiresAt.HasValue &&
               PasswordResetTokenExpiresAt > DateTime.UtcNow;
    }

}
