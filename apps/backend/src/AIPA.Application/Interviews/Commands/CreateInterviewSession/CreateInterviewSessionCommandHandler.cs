using AIPA.Application.Common.Interfaces;
using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Interviews;
using AIPA.Domain.Interviews.Entities;
using AIPA.Domain.Interviews.Repositories;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace AIPA.Application.Interviews.Commands.CreateInterviewSession;

/// <summary>
/// Handler for creating a new interview session
/// </summary>
public class CreateInterviewSessionCommandHandler : IRequestHandler<CreateInterviewSessionCommand, Result<InterviewSessionDto>>
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateInterviewSessionCommandHandler(
        IInterviewSessionRepository sessionRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<InterviewSessionDto>> Handle(CreateInterviewSessionCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists
        var userId = UserId.From(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result<InterviewSessionDto>.Error($"User with ID {request.UserId} not found");
        }

        if (!user.CanScheduleInterview())
        {
            return Result<InterviewSessionDto>.Error("User cannot schedule interviews in current status");
        }

        // Create the interview session
        var session = InterviewSession.Create(
            user,
            request.Title,
            request.Description,
            request.ScheduledAt,
            TimeSpan.FromMinutes(request.DurationMinutes),
            request.Type,
            request.Difficulty);

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sessionDto = _mapper.Map<InterviewSessionDto>(session);
        return Result<InterviewSessionDto>.Success(sessionDto);
    }
}
