using AIPA.Application.Common.Models;
using AIPA.Domain.ValueObjects;
using MediatR;

namespace AIPA.Application.Interviews.Commands.StartInterviewSession;

public class StartInterviewSessionCommand : IRequest<Result>
{
    public Guid SessionId { get; set; }
}
