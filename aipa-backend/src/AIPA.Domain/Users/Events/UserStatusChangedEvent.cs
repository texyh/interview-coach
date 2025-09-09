using AIPA.Domain.Common;
using AIPA.Domain.Enums;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Events;

/// <summary>
/// Domain event raised when user status changes
/// </summary>
public sealed record UserStatusChangedEvent(UserId UserId, AccountStatus NewStatus) : DomainEvent;
