using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Interviews;
using AIPA.Domain.Enums;
using AIPA.Domain.Interviews.Enums;
using MediatR;

namespace AIPA.Application.Interviews.Queries.GetInterviewSessions;

/// <summary>
/// Query to get interview sessions with pagination and filtering
/// </summary>
public class GetInterviewSessionsQuery : IRequest<Result<GetInterviewSessionsResponse>>
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Filter by user ID (optional)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Filter by status (optional)
    /// </summary>
    public InterviewSessionStatus? Status { get; set; }

    /// <summary>
    /// Filter by sessions from this date (optional)
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter by sessions to this date (optional)
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Filter by interview type (optional)
    /// </summary>
    public InterviewType? Type { get; set; }

    /// <summary>
    /// Filter by difficulty level (optional)
    /// </summary>
    public DifficultyLevel? Difficulty { get; set; }
}
