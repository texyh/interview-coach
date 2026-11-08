using AIPA.Application.Common.Interfaces;
using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Interviews;
using AIPA.Domain.Interviews.Repositories;
using AIPA.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace AIPA.Application.Interviews.Commands.UpdateInterviewSession;

/// <summary>
/// Handler for updating an existing interview session
/// </summary>
public class UpdateInterviewSessionCommandHandler : IRequestHandler<UpdateInterviewSessionCommand, Result<InterviewSessionDto>>
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateInterviewSessionCommandHandler(
        IInterviewSessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<InterviewSessionDto>> Handle(UpdateInterviewSessionCommand request, CancellationToken cancellationToken)
    {
        // Get the existing session
        var sessionId = InterviewSessionId.From(request.SessionId);
        var existingSession = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (existingSession == null)
        {
            return Result<InterviewSessionDto>.Error($"Interview session with ID {request.SessionId} was not found");
        }

        // Update session properties
        existingSession.UpdateDetails(
            request.Title,
            request.Description,
            request.ScheduledAt,
            TimeSpan.FromMinutes(request.DurationMinutes),
            request.Type,
            request.Difficulty);

        existingSession.UpdateStatus(request.Status);

        // Save changes
        await _sessionRepository.UpdateAsync(existingSession, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sessionDto = _mapper.Map<InterviewSessionDto>(existingSession);
        return Result<InterviewSessionDto>.Success(sessionDto);
    }
}
