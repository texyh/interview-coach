
using AIPA.Application.Common.Interfaces;
using AIPA.Application.DTOs.Interviews;

namespace AIPA.Application.Interviews.Commands.ScheduleInterviewSession;

/// <summary>
/// Command to schedule a new interview session
/// </summary>
public record ScheduleInterviewSessionCommand(
    Guid UserId,
    string InterviewType,
    DateTime ScheduledAt) : ICommand<InterviewSessionDto>;
