using AIPA.Application.Common.Models;
using AIPA.Domain.ValueObjects;
using MediatR;

namespace AIPA.Application.Interviews.Commands.CompleteInterviewSession;

public class CompleteInterviewSessionCommand : IRequest<Result>
{
    public Guid SessionId { get; set; }
}
