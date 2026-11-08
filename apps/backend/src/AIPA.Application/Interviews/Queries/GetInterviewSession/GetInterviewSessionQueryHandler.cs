using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Interviews;
using AIPA.Domain.Interviews.Repositories;
using AIPA.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace AIPA.Application.Interviews.Queries.GetInterviewSession;

/// <summary>
/// Handler for getting an interview session by ID
/// </summary>
public class GetInterviewSessionQueryHandler : IRequestHandler<GetInterviewSessionQuery, Result<InterviewSessionDto>>
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IMapper _mapper;

    public GetInterviewSessionQueryHandler(IInterviewSessionRepository sessionRepository, IMapper mapper)
    {
        _sessionRepository = sessionRepository;
        _mapper = mapper;
    }

    public async Task<Result<InterviewSessionDto>> Handle(GetInterviewSessionQuery request, CancellationToken cancellationToken)
    {
        var sessionId = InterviewSessionId.From(request.SessionId);
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (session == null)
        {
            return Result<InterviewSessionDto>.Error($"Interview session with ID {request.SessionId} was not found");
        }

        var sessionDto = _mapper.Map<InterviewSessionDto>(session);
        return Result<InterviewSessionDto>.Success(sessionDto);
    }
}
