using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Interviews;
using AIPA.Domain.Enums;
using AIPA.Domain.Interviews.Enums;
using MediatR;

namespace AIPA.Application.Interviews.Commands.CreateInterviewSession;

/// <summary>
/// Command to create a new interview session
/// </summary>
public class CreateInterviewSessionCommand : IRequest<Result<InterviewSessionDto>>
{
    /// <summary>
    /// User ID who owns the session
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Session title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Session description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Scheduled date and time
    /// </summary>
    public DateTime ScheduledAt { get; set; }

    /// <summary>
    /// Duration in minutes
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Interview type
    /// </summary>
    public InterviewType Type { get; set; }

    /// <summary>
    /// Interview difficulty
    /// </summary>
    public DifficultyLevel Difficulty { get; set; }
}
