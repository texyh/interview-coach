using AIPA.Application.DTOs.Auth;
using MediatR;

namespace AIPA.Application.Users.Commands.RegisterUser;

/// <summary>
/// Command to register a new user
/// </summary>
public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    bool AcceptTerms
) : IRequest<RegisterResponse>;
