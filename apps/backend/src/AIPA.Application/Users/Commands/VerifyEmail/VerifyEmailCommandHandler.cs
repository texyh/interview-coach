using AIPA.Application.Common.Interfaces;
using AIPA.Application.DTOs.Auth;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIPA.Application.Users.Commands.VerifyEmail;

/// <summary>
/// Handler for email verification command
/// </summary>
public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, EmailVerificationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<VerifyEmailCommandHandler> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<EmailVerificationResponse> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing email verification for: {Email}", request.Email);

        try
        {
            // Create email value object
            var email = Email.Create(request.Email);

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Email verification attempted for non-existent user: {Email}", request.Email);
                return new EmailVerificationResponse
                {
                    Success = false,
                    Message = "Invalid verification request",
                    Email = request.Email
                };
            }

            // Check if email is already verified
            if (user.IsEmailVerified)
            {
                _logger.LogInformation("Email already verified for user: {Email}", request.Email);
                return new EmailVerificationResponse
                {
                    Success = true,
                    Message = "Email is already verified",
                    Email = request.Email,
                    VerifiedAt = user.EmailVerifiedAt
                };
            }

            // Verify the token and email
            user.VerifyEmail(request.Token);

            // Update user in repository
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(user.Email.Value, user.FullName.DisplayName, cancellationToken);

            _logger.LogInformation("Email verified successfully for user: {Email}", request.Email);

            return new EmailVerificationResponse
            {
                Success = true,
                Message = "Email verified successfully",
                Email = request.Email,
                VerifiedAt = user.EmailVerifiedAt
            };
        }
        catch (Exception ex) when (ex.Message.Contains("Invalid verification token") || ex.Message.Contains("expired"))
        {
            _logger.LogWarning("Email verification failed for {Email}: {Message}", request.Email, ex.Message);
            return new EmailVerificationResponse
            {
                Success = false,
                Message = "Invalid or expired verification token",
                Email = request.Email
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification for {Email}", request.Email);
            throw;
        }
    }
}
