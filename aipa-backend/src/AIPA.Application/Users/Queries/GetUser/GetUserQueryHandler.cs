using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Users;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace AIPA.Application.Users.Queries.GetUser;

/// <summary>
/// Handler for getting a user by ID
/// </summary>
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var userId = UserId.From(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return Result<UserDto>.Error($"User with ID {request.UserId} not found");

        var userDto = _mapper.Map<UserDto>(user);
        return Result<UserDto>.Success(userDto);
    }
}
