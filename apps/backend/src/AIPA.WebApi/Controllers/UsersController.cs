using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Users;
using AIPA.Application.Users.Commands.CreateUser;
using AIPA.Application.Users.Commands.UpdateUser;
using AIPA.Application.Users.Commands.DeleteUser;
using AIPA.Application.Users.Commands.UpdateUserPreferences;
using AIPA.Application.Users.Queries.GetUser;
using AIPA.Application.Users.Queries.GetUsers;
using AIPA.Application.Users.Queries.GetUserByEmail;
using AIPA.Application.Users.Queries.GetUserPreferences;
using AIPA.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIPA.WebApi.Controllers;

/// <summary>
/// Controller for user management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Users")]
public class UsersController : BaseApiController
{
    private readonly ILogger<UsersController> _logger;
    private readonly IMediator _mediator;

    public UsersController(ILogger<UsersController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving users with pagination - Page: {Page}, PageSize: {PageSize}", query.Page, query.PageSize);

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Users retrieval failed: {Error}", result.FirstError);
            return ToActionResult(result);
        }

        return CreatePagedResponse(
            result.Value.Users,
            result.Value.TotalCount,
            query.Page,
            query.PageSize);
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="command">The user creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid user data provided</response>
    /// <response code="409">User with email already exists</response>
    [HttpPost]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user with email: {Email}", command.Email);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("User creation failed: {Error}", result.FirstError);
            return ToActionResult(result);
        }

        _logger.LogInformation("User created successfully with ID: {UserId}", result.Value.Id);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user details</returns>
    /// <response code="200">User retrieved successfully</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving user with ID: {UserId}", id);

        var query = new GetUserQuery { UserId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("User retrieval failed for ID {UserId}: {Error}", id, result.FirstError);
            return ToActionResult(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get user by email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    [HttpGet("by-email")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving user with email: {Email}", email);

        var query = new GetUserByEmailQuery { Email = email };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("User retrieval failed for email {Email}: {Error}", email, result.FirstError);
            return ToActionResult(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="command">Update user command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user details</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        if (id != command.UserId)
        {
            _logger.LogWarning("User ID mismatch: URL ID {UrlId} vs Command ID {CommandId}", id, command.UserId);
            return BadRequest("User ID in URL does not match the command");
        }

        _logger.LogInformation("Updating user with ID: {UserId}", id);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("User update failed for ID {UserId}: {Error}", id, result.FirstError);
            return ToActionResult(result);
        }

        _logger.LogInformation("User updated successfully with ID: {UserId}", id);
        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", id);

        var command = new DeleteUserCommand { UserId = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("User deletion failed for ID {UserId}: {Error}", id, result.FirstError);
            return ToActionResult(result);
        }

        _logger.LogInformation("User deleted successfully with ID: {UserId}", id);
        return NoContent();
    }

    /// <summary>
    /// Gets user preferences by user ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User preferences</returns>
    [HttpGet("{id}/preferences")]
    [Authorize]
    public async Task<ActionResult<UserPreferencesDto>> GetUserPreferences(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting preferences for user with ID: {UserId}", id);

            var query = new GetUserPreferencesQuery(id);
            var preferences = await _mediator.Send(query);

            return Ok(preferences);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("User not found for preferences retrieval: {UserId}", id);
            return NotFound(new { message = $"User with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preferences for user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving user preferences" });
        }
    }

    /// <summary>
    /// Updates user preferences
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">User preferences update request</param>
    /// <returns>Updated user preferences</returns>
    [HttpPut("{id}/preferences")]
    [Authorize]
    public async Task<ActionResult<UserPreferencesDto>> UpdateUserPreferences(
        Guid id,
        [FromBody] UpdateUserPreferencesRequest request)
    {
        try
        {
            _logger.LogInformation("Updating preferences for user with ID: {UserId}", id);

            var command = new UpdateUserPreferencesCommand(
                id,
                request.EmailNotifications,
                request.PushNotifications,
                request.WeeklyReports,
                request.Language,
                request.TimeZone,
                request.Theme,
                request.NotificationInterviewTypes,
                request.ShareAnalytics,
                request.PublicProfile,
                request.SessionReminderMinutes);

            var preferences = await _mediator.Send(command);

            _logger.LogInformation("Preferences updated successfully for user: {UserId}", id);
            return Ok(preferences);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("User not found for preferences update: {UserId}", id);
            return NotFound(new { message = $"User with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating preferences for user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating user preferences" });
        }
    }
}
