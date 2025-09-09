using AIPA.Application.Common.Interfaces;
using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Users;
using AIPA.Domain.Users.Repositories;
using AutoMapper;
using MediatR;

namespace AIPA.Application.Users.Queries.GetUsers;

/// <summary>
/// Handler for getting users with pagination
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<GetUsersResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<GetUsersResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (users, totalCount) = await _userRepository.GetUsersPagedAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            cancellationToken);

        var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

        var response = new GetUsersResponse
        {
            Users = userDtos,
            TotalCount = totalCount
        };

        return Result<GetUsersResponse>.Success(response);
    }
}
