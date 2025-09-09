using AIPA.Application.DTOs.Auth;
using MediatR;

namespace AIPA.Application.Users.Commands.VerifyEmail;

/// <summary>
/// Command to verify user's email address
/// </summary>
public record VerifyEmailCommand(
    string Email,
    string Token
) : IRequest<EmailVerificationResponse>;
