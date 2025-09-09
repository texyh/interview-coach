using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Interviews;
using MediatR;

namespace AIPA.Application.Interviews.Queries.GetInterviewSession;

/// <summary>
/// Query to get an interview session by ID
/// </summary>
public class GetInterviewSessionQuery : IRequest<Result<InterviewSessionDto>>
{
    /// <summary>
    /// Session ID
    /// </summary>
    public Guid SessionId { get; set; }
}
