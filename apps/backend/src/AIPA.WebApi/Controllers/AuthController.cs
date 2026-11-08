using AIPA.Application.Common.Interfaces;
using AIPA.Application.DTOs.Auth;
using AIPA.Application.Users.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AIPA.WebApi.Controllers;

/// <summary>
/// Authentication controller for user login, logout, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Authentication endpoints for user login, logout, and token refresh")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        IMediator mediator,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="request">Registration request containing user details and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration response with user information</returns>
    /// <response code="201">Registration successful</response>
    /// <response code="400">Invalid request or email already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "User registration",
        Description = "Registers a new user account with email verification")]
    [SwaggerResponse(201, "Registration successful", typeof(RegisterResponse))]
    [SwaggerResponse(400, "Invalid request or email already exists", typeof(ValidationProblemDetails))]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.AcceptTerms
            );

            var response = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("User {Email} registered successfully with ID {UserId}",
                request.Email, response.UserId);

            return CreatedAtAction(nameof(Register), new { id = response.UserId }, response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning("Registration failed - email already exists: {Email}", request.Email);
            return BadRequest(new { message = "A user with this email address already exists" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Verifies a user's email address
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="token">Email verification token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email verification result</returns>
    /// <response code="200">Email verified successfully</response>
    /// <response code="400">Invalid or expired verification token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Verify email address",
        Description = "Verifies a user's email address using the verification token")]
    [SwaggerResponse(200, "Email verified successfully", typeof(EmailVerificationResponse))]
    [SwaggerResponse(400, "Invalid or expired verification token")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string email,
        [FromQuery] string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "Email and token are required" });
            }

            var command = new AIPA.Application.Users.Commands.VerifyEmail.VerifyEmailCommand(email, token);
            var response = await _mediator.Send(command, cancellationToken);

            if (response.Success)
            {
                _logger.LogInformation("Email verified successfully for {Email}", email);
                return Ok(response);
            }
            else
            {
                _logger.LogWarning("Email verification failed for {Email}: {Message}", email, response.Message);
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification for {Email}", email);
            return StatusCode(500, new { message = "An error occurred during email verification" });
        }
    }

    /// <summary>
    /// Initiates password reset process
    /// </summary>
    /// <param name="request">Forgot password request containing email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Password reset initiation result</returns>
    /// <response code="200">Password reset email sent (if email exists)</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Initiate password reset",
        Description = "Sends a password reset email to the user if the email exists")]
    [SwaggerResponse(200, "Password reset email sent")]
    [SwaggerResponse(400, "Invalid request", typeof(ValidationProblemDetails))]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new AIPA.Application.Users.Commands.ForgotPassword.ForgotPasswordCommand(request.Email);
            var success = await _mediator.Send(command, cancellationToken);

            // Always return success for security reasons
            _logger.LogInformation("Password reset process initiated for {Email}", request.Email);
            return Ok(new { message = "If the email exists, a password reset link has been sent." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password process for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during the password reset process" });
        }
    }

    /// <summary>
    /// Resets user password using reset token
    /// </summary>
    /// <param name="request">Reset password request containing email, token, and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Password reset result</returns>
    /// <response code="200">Password reset successful</response>
    /// <response code="400">Invalid request or expired token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Reset password",
        Description = "Resets user password using the reset token")]
    [SwaggerResponse(200, "Password reset successful")]
    [SwaggerResponse(400, "Invalid request or expired token", typeof(ValidationProblemDetails))]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new AIPA.Application.Users.Commands.ResetPassword.ResetPasswordCommand(
                request.Email,
                request.Token,
                request.Password);

            var success = await _mediator.Send(command, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Password reset successfully for {Email}", request.Email);
                return Ok(new { message = "Password reset successfully" });
            }
            else
            {
                _logger.LogWarning("Password reset failed for {Email}", request.Email);
                return BadRequest(new { message = "Invalid or expired reset token" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during password reset" });
        }
    }

    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    /// <param name="request">Authentication request containing email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT tokens and user information</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Invalid credentials or inactive account</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "User login",
        Description = "Authenticates a user with email and password, returning JWT access and refresh tokens")]
    [SwaggerResponse(200, "Authentication successful", typeof(AuthenticationResponse))]
    [SwaggerResponse(400, "Invalid request", typeof(ValidationProblemDetails))]
    [SwaggerResponse(401, "Invalid credentials or inactive account")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> Login(
        [FromBody] AuthenticationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authenticationService.AuthenticateAsync(request, cancellationToken);

            _logger.LogInformation("User {Email} logged in successfully", request.Email);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Authentication failed for {Email}: {Message}", request.Email, ex.Message);
            return Unauthorized(new { message = "Invalid email or password" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during authentication" });
        }
    }

    /// <summary>
    /// Refreshes an access token using a refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New authentication response with fresh tokens</returns>
    /// <response code="200">Token refresh successful</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Invalid or expired refresh token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Refresh access token",
        Description = "Refreshes an access token using a valid refresh token")]
    [SwaggerResponse(200, "Token refresh successful", typeof(AuthenticationResponse))]
    [SwaggerResponse(400, "Invalid request", typeof(ValidationProblemDetails))]
    [SwaggerResponse(401, "Invalid or expired refresh token")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authenticationService.RefreshTokenAsync(request, cancellationToken);

            _logger.LogInformation("Token refreshed successfully");

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new { message = "Invalid refresh token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    /// <param name="request">Logout request containing refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Logout result</returns>
    /// <response code="200">Logout successful</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("logout")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "User logout",
        Description = "Logs out a user by invalidating their refresh token")]
    [SwaggerResponse(200, "Logout successful")]
    [SwaggerResponse(400, "Invalid request", typeof(ValidationProblemDetails))]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _authenticationService.LogoutAsync(request, cancellationToken);

            if (success)
            {
                _logger.LogInformation("User logged out successfully");
                return Ok(new { message = "Logged out successfully" });
            }
            else
            {
                _logger.LogWarning("Logout failed");
                return BadRequest(new { message = "Logout failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Gets the current user's profile information
    /// </summary>
    /// <returns>Current user information</returns>
    /// <response code="200">User profile retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Get current user profile",
        Description = "Retrieves the current authenticated user's profile information")]
    [SwaggerResponse(200, "User profile retrieved successfully")]
    [SwaggerResponse(401, "User not authenticated")]
    [SwaggerResponse(500, "Internal server error")]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            var permissions = User.FindAll("permission").Select(c => c.Value).ToList();

            var userInfo = new
            {
                Id = userId,
                Email = email,
                Name = name,
                Roles = roles,
                Permissions = permissions
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user information");
            return StatusCode(500, new { message = "An error occurred while retrieving user information" });
        }
    }
}
