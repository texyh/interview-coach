using AIPA.Application.Common.Interfaces;
using AIPA.Application.Common.Models;
using AIPA.Domain.Interviews.Repositories;
using AIPA.Domain.ValueObjects;
using MediatR;

namespace AIPA.Application.Interviews.Commands.DeleteInterviewSession;

/// <summary>
/// Handler for deleting an interview session
/// </summary>
public class DeleteInterviewSessionCommandHandler : IRequestHandler<DeleteInterviewSessionCommand, Result>
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteInterviewSessionCommandHandler(
        IInterviewSessionRepository sessionRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteInterviewSessionCommand request, CancellationToken cancellationToken)
    {
        var sessionId = InterviewSessionId.From(request.SessionId);
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (session == null)
        {
            return Result.Error($"Interview session with ID {request.SessionId} not found");
        }

        // Cancel the session instead of hard delete
        session.Cancel("Session deleted by user request");

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
