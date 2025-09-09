using AIPA.Application.Common.Models;
using AIPA.Domain.ValueObjects;
using MediatR;

namespace AIPA.Application.Interviews.Commands.PauseInterviewSession;

public class PauseInterviewSessionCommand : IRequest<Result>
{
    public Guid SessionId { get; set; }
}
