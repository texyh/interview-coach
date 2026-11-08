using AIPA.Application.DTOs.Interviews;

namespace AIPA.Application.Interviews.Queries.GetInterviewSessions;

/// <summary>
/// Response for GetInterviewSessions query
/// </summary>
public class GetInterviewSessionsResponse
{
    /// <summary>
    /// Collection of interview sessions
    /// </summary>
    public IEnumerable<InterviewSessionDto> Sessions { get; set; } = new List<InterviewSessionDto>();

    /// <summary>
    /// Total count of sessions matching the query
    /// </summary>
    public int TotalCount { get; set; }
}
