using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Interviews;
using AIPA.Domain.Enums;
using AIPA.Domain.Interviews.Enums;
using MediatR;

namespace AIPA.Application.Interviews.Commands.UpdateInterviewSession;

/// <summary>
/// Command to update an existing interview session
/// </summary>
public class UpdateInterviewSessionCommand : IRequest<Result<InterviewSessionDto>>
{
    /// <summary>
    /// Session ID to update
    /// </summary>
    public Guid SessionId { get; set; }

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

    /// <summary>
    /// Interview status
    /// </summary>
    public InterviewSessionStatus Status { get; set; }
}
