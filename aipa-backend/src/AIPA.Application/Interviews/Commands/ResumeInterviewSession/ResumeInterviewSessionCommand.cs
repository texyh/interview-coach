using AIPA.Application.Common.Models;
using AIPA.Domain.ValueObjects;
using MediatR;

namespace AIPA.Application.Interviews.Commands.ResumeInterviewSession;

public class ResumeInterviewSessionCommand : IRequest<Result>
{
    public Guid SessionId { get; set; }
}
