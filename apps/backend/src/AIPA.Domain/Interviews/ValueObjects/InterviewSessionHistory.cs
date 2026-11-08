namespace AIPA.Domain.Interviews.ValueObjects;

public class InterviewSessionHistory
{
    public List<SessionEvent> Events { get; private set; } = new();
    public SessionAnalytics Analytics { get; private set; } = new();

    private InterviewSessionHistory() { }

    public static InterviewSessionHistory Create()
    {
        return new InterviewSessionHistory();
    }

    public void AddEvent(string eventType, string description, Dictionary<string, object>? metadata = null)
    {
        Events.Add(SessionEvent.Create(eventType, description, metadata));
    }

    public void UpdateAnalytics(Action<SessionAnalytics> updateAction)
    {
        updateAction(Analytics);
    }
}

public class SessionEvent
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string EventType { get; private set; }
    public string Description { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    private SessionEvent()
    {
        EventType = string.Empty;
        Description = string.Empty;
    }

    public static SessionEvent Create(string eventType, string description, Dictionary<string, object>? metadata = null)
    {
        return new SessionEvent
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            EventType = eventType,
            Description = description,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
}

public class SessionAnalytics
{
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan ActiveTime { get; set; }
    public TimeSpan PausedTime { get; set; }
    public int PauseCount { get; set; }
    public int QuestionsAsked { get; set; }
    public int QuestionsAnswered { get; set; }
    public double OverallScore { get; set; }
    public Dictionary<string, double> SkillScores { get; set; } = new();
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? LastResponseAt { get; set; }
    public TimeSpan AverageResponseTime { get; set; }

    public void IncrementPause()
    {
        PauseCount++;
    }

    public void AddQuestionAsked()
    {
        QuestionsAsked++;
    }

    public void AddQuestionAnswered()
    {
        QuestionsAnswered++;
        if (FirstResponseAt == null)
            FirstResponseAt = DateTime.UtcNow;
        LastResponseAt = DateTime.UtcNow;
    }

    public void UpdateSkillScore(string skill, double score)
    {
        SkillScores[skill] = score;
    }

    public void AddPerformanceMetric(string metric, object value)
    {
        PerformanceMetrics[metric] = value;
    }
}
