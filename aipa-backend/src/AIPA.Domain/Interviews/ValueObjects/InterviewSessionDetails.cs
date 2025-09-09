using AIPA.Domain.Enums;
using AIPA.Domain.Interviews.Enums;
using AIPA.Domain.Common.Exceptions;

namespace AIPA.Domain.Interviews.ValueObjects;

public record InterviewSessionDetails(
    string Title,
    string? Description,
    TimeSpan Duration,
    InterviewType Type,
    DifficultyLevel Difficulty)
{
    public static InterviewSessionDetails Create(
        string title,
        string? description,
        TimeSpan duration,
        InterviewType type,
        DifficultyLevel difficulty)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException("Interview session title cannot be empty");

        if (duration <= TimeSpan.Zero)
            throw new BusinessRuleValidationException("Interview session duration must be positive");

        return new InterviewSessionDetails(title, description, duration, type, difficulty);
    }
}
