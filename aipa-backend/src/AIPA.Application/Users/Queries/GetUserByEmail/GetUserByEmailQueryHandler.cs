using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Users;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace AIPA.Application.Users.Queries.GetUserByEmail;

/// <summary>
/// Handler for getting a user by email
/// </summary>
public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserByEmailQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Error($"User with email {request.Email} was not found");
        }

        var userDto = _mapper.Map<UserDto>(user);
        return Result<UserDto>.Success(userDto);
    }
}
