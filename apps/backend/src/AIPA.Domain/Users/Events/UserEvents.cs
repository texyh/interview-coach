using AIPA.Domain.Common;
using AIPA.Domain.Enums;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Events;

public record UserCreatedEvent(UserId UserId, Email Email) : DomainEvent;

public record UserProfileUpdatedEvent(UserId UserId, UserProfile Profile) : DomainEvent;

public record UserPreferencesUpdatedEvent(UserId UserId, UserPreferences Preferences) : DomainEvent;

public record UserRoleAssignedEvent(UserId UserId, RoleId RoleId, string RoleName) : DomainEvent;

public record UserRoleRemovedEvent(UserId UserId, RoleId RoleId, string RoleName) : DomainEvent;

public record UserSubscriptionChangedEvent(UserId UserId, SubscriptionId SubscriptionId, SubscriptionTier Tier) : DomainEvent;

public record UserEmailVerifiedEvent(UserId UserId, Email Email) : DomainEvent;

public record UserPasswordResetEvent(UserId UserId, Email Email) : DomainEvent;
