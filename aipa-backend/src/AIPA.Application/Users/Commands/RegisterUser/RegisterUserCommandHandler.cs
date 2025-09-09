using AIPA.Application.Common.Interfaces;
using AIPA.Application.DTOs.Auth;
using AIPA.Domain.Users.Entities;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIPA.Application.Users.Commands.RegisterUser;

/// <summary>
/// Handler for user registration command
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHashingService passwordHashingService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHashingService = passwordHashingService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing user registration for email: {Email}", request.Email);

        // Create email value object for checking existing user
        var emailValueObject = Email.Create(request.Email);

        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(emailValueObject, cancellationToken);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration attempted for existing email: {Email}", request.Email);
            throw new InvalidOperationException("A user with this email address already exists");
        }

        // Create value objects
        var email = Email.Create(request.Email);
        var fullName = FullName.Create(request.FirstName, request.LastName);

        // Create user
        var user = User.Create(email, fullName);

        // Hash password and set it
        var passwordHash = _passwordHashingService.HashPassword(request.Password);
        user.SetPasswordHash(passwordHash);

        // Generate email verification token
        user.GenerateEmailVerificationToken();

        // Save user
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send email verification email
        if (!string.IsNullOrEmpty(user.EmailVerificationToken))
        {
            await _emailService.SendEmailVerificationAsync(
                user.Email.Value,
                user.FullName.DisplayName,
                user.EmailVerificationToken,
                cancellationToken);
        }

        _logger.LogInformation("User registered successfully with ID: {UserId}", user.Id.Value);

        return new RegisterResponse
        {
            UserId = user.Id.Value,
            Email = user.Email.Value,
            FullName = user.FullName.DisplayName,
            EmailVerificationRequired = true,
            Message = "Registration successful. Please check your email to verify your account.",
            RegisteredAt = user.CreatedAt
        };
    }
}
