namespace AIPA.Application.DTOs.Interviews;

/// <summary>
/// Interview session data transfer object
/// </summary>
public record InterviewSessionDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime ScheduledAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public List<InterviewQuestionDto> Questions { get; init; } = new();
    public TimeSpan? Duration { get; init; }
    public decimal? Score { get; init; }
}

/// <summary>
/// Interview question data transfer object
/// </summary>
public record InterviewQuestionDto
{
    public Guid Id { get; init; }
    public string Question { get; init; } = string.Empty;
    public string? Answer { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public int Order { get; init; }
    public TimeSpan? TimeSpent { get; init; }
    public decimal? Score { get; init; }
}

/// <summary>
/// Simplified interview session data transfer object for lists
/// </summary>
public record InterviewSessionSummaryDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime ScheduledAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public decimal? Score { get; init; }
}
