using AIPA.Application.DTOs.Auth;
using MediatR;

namespace AIPA.Application.Users.Commands.ResetPassword;

/// <summary>
/// Command to reset user password
/// </summary>
public record ResetPasswordCommand(
    string Email,
    string Token,
    string Password
) : IRequest<bool>;
