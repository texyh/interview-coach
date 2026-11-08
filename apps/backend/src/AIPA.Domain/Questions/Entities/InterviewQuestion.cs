using AIPA.Domain.Common;
using AIPA.Domain.Common.Exceptions;
using AIPA.Domain.Enums;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Questions.Entities;

public class InterviewQuestion : BaseEntity
{
    public QuestionId Id { get; private set; }
    public string Text { get; private set; }
    public QuestionType Type { get; private set; }
    public QuestionDifficulty Difficulty { get; private set; }
    public List<string> Tags { get; private set; }
    public string? ExpectedAnswer { get; private set; }
    public string? UserAnswer { get; private set; }
    public bool IsAnswered { get; private set; }
    public DateTime? AnsweredAt { get; private set; }

    private InterviewQuestion()
    {
        Id = null!;
        Text = null!;
        Tags = [];
    } // For EF Core

    private InterviewQuestion(
        QuestionId id,
        string text,
        QuestionType type,
        QuestionDifficulty difficulty,
        List<string> tags,
        string? expectedAnswer)
    {
        Id = id;
        Text = text;
        Type = type;
        Difficulty = difficulty;
        Tags = tags ?? [];
        ExpectedAnswer = expectedAnswer;
        IsAnswered = false;
    }

    public static InterviewQuestion Create(
        string text,
        QuestionType type,
        QuestionDifficulty difficulty,
        List<string>? tags = null,
        string? expectedAnswer = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new BusinessRuleValidationException("Question text cannot be empty");

        return new InterviewQuestion(
            QuestionId.New(),
            text.Trim(),
            type,
            difficulty,
            tags ?? [],
            expectedAnswer?.Trim());
    }

    public void Answer(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
            throw new BusinessRuleValidationException("Answer cannot be empty");

        if (IsAnswered)
            throw new BusinessRuleValidationException("Question has already been answered");

        UserAnswer = answer.Trim();
        IsAnswered = true;
        AnsweredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAnswer(string newAnswer)
    {
        if (string.IsNullOrWhiteSpace(newAnswer))
            throw new BusinessRuleValidationException("Answer cannot be empty");

        UserAnswer = newAnswer.Trim();
        AnsweredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearAnswer()
    {
        UserAnswer = null;
        IsAnswered = false;
        AnsweredAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new BusinessRuleValidationException("Tag cannot be empty");

        var normalizedTag = tag.Trim().ToLowerInvariant();
        if (!Tags.Contains(normalizedTag))
        {
            Tags.Add(normalizedTag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        var normalizedTag = tag.Trim().ToLowerInvariant();
        if (Tags.Remove(normalizedTag))
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
