using AIPA.Application.Common.Models;
using AIPA.Domain.Interviews.Repositories;
using AIPA.Domain.Interviews.Services;
using AIPA.Domain.ValueObjects;
using MediatR;

namespace AIPA.Application.Interviews.Commands.CompleteInterviewSession;

public class CompleteInterviewSessionCommandHandler : IRequestHandler<CompleteInterviewSessionCommand, Result>
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IInterviewSessionSecurityService _securityService;

    public CompleteInterviewSessionCommandHandler(
        IInterviewSessionRepository sessionRepository,
        IInterviewSessionSecurityService securityService)
    {
        _sessionRepository = sessionRepository;
        _securityService = securityService;
    }

    public async Task<Result> Handle(CompleteInterviewSessionCommand request, CancellationToken cancellationToken)
    {
        var sessionId = InterviewSessionId.From(request.SessionId);
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (session == null)
        {
            return Result.Error("Interview session not found");
        }

        if (!await _securityService.CanUserModifySessionAsync(sessionId, session.User.Id, cancellationToken))
        {
            return Result.Error("User cannot modify this session");
        }

        if (!await _securityService.ValidateSessionStateTransitionAsync(session, Domain.Interviews.Enums.InterviewSessionState.Completed))
        {
            return Result.Error("Invalid session state transition");
        }

        session.Complete();
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        return Result.Success();
    }
}
