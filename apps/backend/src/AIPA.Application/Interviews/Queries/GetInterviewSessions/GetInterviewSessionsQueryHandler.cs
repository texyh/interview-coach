using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Interviews;
using AIPA.Domain.Interviews.Repositories;
using AIPA.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace AIPA.Application.Interviews.Queries.GetInterviewSessions;

/// <summary>
/// Handler for getting interview sessions with pagination and filtering
/// </summary>
public class GetInterviewSessionsQueryHandler : IRequestHandler<GetInterviewSessionsQuery, Result<GetInterviewSessionsResponse>>
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IMapper _mapper;

    public GetInterviewSessionsQueryHandler(IInterviewSessionRepository sessionRepository, IMapper mapper)
    {
        _sessionRepository = sessionRepository;
        _mapper = mapper;
    }

    public async Task<Result<GetInterviewSessionsResponse>> Handle(GetInterviewSessionsQuery request, CancellationToken cancellationToken)
    {
        UserId? userId = null;
        if (request.UserId.HasValue)
        {
            userId = UserId.From(request.UserId.Value);
        }

        var (sessions, totalCount) = await _sessionRepository.GetSessionsPagedAsync(
            request.Page,
            request.PageSize,
            userId,
            request.Status,
            cancellationToken);

        var sessionDtos = _mapper.Map<IEnumerable<InterviewSessionDto>>(sessions);

        var response = new GetInterviewSessionsResponse
        {
            Sessions = sessionDtos,
            TotalCount = totalCount
        };

        return Result<GetInterviewSessionsResponse>.Success(response);
    }
}
