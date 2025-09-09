using AIPA.Domain.Common;
using AIPA.Domain.ValueObjects;
using AIPA.Domain.Interviews.Enums;

namespace AIPA.Domain.Interviews.Events;

public record InterviewSessionScheduledEvent(
    InterviewSessionId SessionId,
    UserId UserId,
    DateTime ScheduledAt) : DomainEvent;

public record InterviewSessionCreatedEvent(
    InterviewSessionId SessionId,
    UserId UserId) : DomainEvent;

public record InterviewSessionUpdatedEvent(
    InterviewSessionId SessionId) : DomainEvent;

public record InterviewSessionStatusChangedEvent(
    InterviewSessionId SessionId,
    InterviewSessionStatus NewStatus) : DomainEvent;

public record InterviewSessionStartedEvent(
    InterviewSessionId SessionId,
    UserId UserId) : DomainEvent;

public record InterviewSessionPausedEvent(
    InterviewSessionId SessionId,
    UserId UserId) : DomainEvent;

public record InterviewSessionResumedEvent(
    InterviewSessionId SessionId,
    UserId UserId) : DomainEvent;

public record InterviewSessionCompletedEvent(
    InterviewSessionId SessionId,
    UserId UserId) : DomainEvent;

public record InterviewSessionCancelledEvent(
    InterviewSessionId SessionId,
    UserId UserId,
    string Reason) : DomainEvent;
