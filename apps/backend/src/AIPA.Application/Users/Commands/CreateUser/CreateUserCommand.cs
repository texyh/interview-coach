using AIPA.Application.Common.Interfaces;
using AIPA.Application.DTOs.Users;

namespace AIPA.Application.Users.Commands.CreateUser;

/// <summary>
/// Command to create a new user
/// </summary>
public record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName) : ICommand<UserDto>;
