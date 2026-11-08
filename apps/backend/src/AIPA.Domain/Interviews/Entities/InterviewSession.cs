using AIPA.Domain.Common;
using AIPA.Domain.Common.Exceptions;
using AIPA.Domain.Enums;
using AIPA.Domain.Interviews.Enums;
using AIPA.Domain.Interviews.Events;
using AIPA.Domain.Interviews.ValueObjects;
using AIPA.Domain.Users.Entities;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Interviews.Entities;

public class InterviewSession : BaseEntity
{
    public InterviewSessionId Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public TimeSpan Duration { get; private set; }
    public InterviewType Type { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    public InterviewSessionStatus Status { get; private set; }
    public InterviewSessionState CurrentState { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? PausedAt { get; private set; }
    public DateTime? ResumedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }

    public SessionRecordingMetadata? RecordingMetadata { get; private set; }
    public InterviewSessionHistory History { get; private set; }

    public User User { get; private set; }

    public InterviewSession()
    {
        Id = null!;
        Title = string.Empty;
        User = null!;
        CurrentState = InterviewSessionState.NotStarted;
        History = InterviewSessionHistory.Create();
    }

    private InterviewSession(
        InterviewSessionId id,
        User user,
        InterviewSessionDetails details,
        DateTime scheduledAt)
    {
        Id = id;
        User = user ?? throw new ArgumentNullException(nameof(user));
        Title = details.Title ?? throw new ArgumentNullException(nameof(details));
        Description = details.Description;
        Duration = details.Duration;
        Type = details.Type;
        Difficulty = details.Difficulty;
        ScheduledAt = scheduledAt;
        Status = InterviewSessionStatus.Scheduled;
        CurrentState = InterviewSessionState.NotStarted;
        History = InterviewSessionHistory.Create();
    }

    public static InterviewSession Create(
        User user,
        string title,
        string? description,
        DateTime scheduledAt,
        TimeSpan duration,
        InterviewType type,
        DifficultyLevel difficulty)
    {


        if (duration <= TimeSpan.Zero)
            throw new BusinessRuleValidationException("Interview session duration must be positive");

        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException("Interview session title cannot be empty");

        if (scheduledAt <= DateTime.UtcNow)
            throw new BusinessRuleValidationException("Interview cannot be scheduled in the past");

        if (duration <= TimeSpan.Zero)
            throw new BusinessRuleValidationException("Interview session duration must be positive");


        var id = InterviewSessionId.New();
        var details = InterviewSessionDetails.Create(
            title,
            description,
            duration,
            type,
            difficulty);

        var session = new InterviewSession(id, user, details, scheduledAt);
        session.AddDomainEvent(new InterviewSessionCreatedEvent(session.Id, user.Id));
        session.History.AddEvent("SessionCreated", $"Interview session '{title}' created for {scheduledAt}");

        return session;
    }

    public void Start()
    {
        if (CurrentState != InterviewSessionState.NotStarted)
            throw new BusinessRuleValidationException("Session can only be started from NotStarted state");

        CurrentState = InterviewSessionState.InProgress;
        Status = InterviewSessionStatus.InProgress;
        StartedAt = DateTime.UtcNow;

        RecordingMetadata = SessionRecordingMetadata.Create();
        RecordingMetadata.StartRecording();

        History.AddEvent("SessionStarted", "Interview session started");
        AddDomainEvent(new InterviewSessionStartedEvent(Id, User.Id));
    }

    public void Pause()
    {
        if (CurrentState != InterviewSessionState.InProgress)
            throw new BusinessRuleValidationException("Session can only be paused from InProgress state");

        CurrentState = InterviewSessionState.Paused;
        PausedAt = DateTime.UtcNow;

        History.AddEvent("SessionPaused", "Interview session paused");
        History.UpdateAnalytics(a => a.IncrementPause());

        AddDomainEvent(new InterviewSessionPausedEvent(Id, User.Id));
    }

    public void Resume()
    {
        if (CurrentState != InterviewSessionState.Paused)
            throw new BusinessRuleValidationException("Session can only be resumed from Paused state");

        CurrentState = InterviewSessionState.InProgress;
        ResumedAt = DateTime.UtcNow;

        History.AddEvent("SessionResumed", "Interview session resumed");
        AddDomainEvent(new InterviewSessionResumedEvent(Id, User.Id));
    }

    public void Complete()
    {
        if (CurrentState != InterviewSessionState.InProgress && CurrentState != InterviewSessionState.Paused)
            throw new BusinessRuleValidationException("Session can only be completed from InProgress or Paused state");

        CurrentState = InterviewSessionState.Completed;
        Status = InterviewSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        var totalDuration = CompletedAt.Value - StartedAt!.Value;
        RecordingMetadata?.CompleteRecording(totalDuration);

        History.AddEvent("SessionCompleted", "Interview session completed");
        History.UpdateAnalytics(a => a.TotalDuration = totalDuration);

        AddDomainEvent(new InterviewSessionCompletedEvent(Id, User.Id));
    }

    public void Cancel(string reason)
    {
        if (CurrentState == InterviewSessionState.Completed)
            throw new BusinessRuleValidationException("Cannot cancel a completed session");

        CurrentState = InterviewSessionState.Cancelled;
        Status = InterviewSessionStatus.Cancelled;

        History.AddEvent("SessionCancelled", $"Interview session cancelled: {reason}");
        AddDomainEvent(new InterviewSessionCancelledEvent(Id, User.Id, reason));
    }

    public void UpdateDetails(
        string title,
        string? description,
        DateTime scheduledAt,
        TimeSpan duration,
        InterviewType type,
        DifficultyLevel difficulty)
    {
        if (CurrentState == InterviewSessionState.InProgress)
            throw new BusinessRuleValidationException("Cannot update details while session is in progress");

        Title = title;
        Description = description;
        ScheduledAt = scheduledAt;
        Duration = duration;
        Type = type;
        Difficulty = difficulty;

        History.AddEvent("SessionDetailsUpdated", "Session details updated");
    }

    public void UpdateStatus(InterviewSessionStatus status)
    {
        if (CurrentState == InterviewSessionState.InProgress)
            throw new BusinessRuleValidationException("Cannot update status while session is in progress");

        Status = status;
        History.AddEvent("StatusUpdated", $"Session status updated to {status}");
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
        History.AddEvent("NotesAdded", "Notes added to session");
    }

    public bool CanStart() => CurrentState == InterviewSessionState.NotStarted && DateTime.UtcNow >= ScheduledAt.AddMinutes(-15);
    public bool CanPause() => CurrentState == InterviewSessionState.InProgress;
    public bool CanResume() => CurrentState == InterviewSessionState.Paused;
    public bool CanComplete() => CurrentState == InterviewSessionState.InProgress || CurrentState == InterviewSessionState.Paused;
    public bool IsActive() => CurrentState == InterviewSessionState.InProgress || CurrentState == InterviewSessionState.Paused;
}
