using AIPA.Application.Common.Interfaces;
using AIPA.Application.DTOs.Users;

namespace AIPA.Application.Users.Commands.UpdateUser;

/// <summary>
/// Command to update an existing user
/// </summary>
public record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    UserProfileDto? Profile = null) : ICommand<UserDto>;
