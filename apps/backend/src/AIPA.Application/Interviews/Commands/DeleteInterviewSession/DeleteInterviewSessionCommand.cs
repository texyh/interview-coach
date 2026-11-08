using AIPA.Application.Common.Models;
using MediatR;

namespace AIPA.Application.Interviews.Commands.DeleteInterviewSession;

/// <summary>
/// Command to delete an interview session
/// </summary>
public class DeleteInterviewSessionCommand : IRequest<Result>
{
    /// <summary>
    /// Session ID to delete
    /// </summary>
    public Guid SessionId { get; set; }
}
