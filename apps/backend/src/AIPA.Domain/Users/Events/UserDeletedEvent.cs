using AIPA.Domain.Common;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Events;

/// <summary>
/// Domain event raised when user is deleted
/// </summary>
public sealed record UserDeletedEvent(UserId UserId) : DomainEvent;
