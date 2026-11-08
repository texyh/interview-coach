using AIPA.Application.DTOs.Auth;
using MediatR;

namespace AIPA.Application.Users.Commands.ForgotPassword;

/// <summary>
/// Command to initiate password reset process
/// </summary>
public record ForgotPasswordCommand(
    string Email
) : IRequest<bool>;
