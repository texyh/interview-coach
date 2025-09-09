using AIPA.Application.Common.Interfaces;
using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Users;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AIPA.Application.Users.Queries.GetUserById;

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Retrieving user with ID: {UserId}", request.UserId);

            var userId = UserId.From(request.UserId);
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", request.UserId);
                return Result<UserDto>.Failure("User not found.");
            }

            var userDto = _mapper.Map<UserDto>(user);

            _logger.LogDebug("Successfully retrieved user with ID: {UserId}", request.UserId);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", request.UserId);
            return Result<UserDto>.Failure("An error occurred while retrieving the user.");
        }
    }
}
