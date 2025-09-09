using Microsoft.AspNetCore.Mvc;
using AIPA.Application.Interviews.Commands.CreateInterviewSession;
using AIPA.Application.Interviews.Commands.UpdateInterviewSession;
using AIPA.Application.Interviews.Commands.DeleteInterviewSession;
using AIPA.Application.Interviews.Commands.StartInterviewSession;
using AIPA.Application.Interviews.Commands.PauseInterviewSession;
using AIPA.Application.Interviews.Commands.ResumeInterviewSession;
using AIPA.Application.Interviews.Commands.CompleteInterviewSession;
using AIPA.Application.Interviews.Queries.GetInterviewSession;
using AIPA.Application.Interviews.Queries.GetInterviewSessions;
using AIPA.WebApi.Controllers.Common;
using MediatR;

namespace AIPA.WebApi.Controllers;

/// <summary>
/// Interview sessions management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Interview Sessions")]
public class InterviewSessionsController : BaseApiController
{
    private readonly ILogger<InterviewSessionsController> _logger;
    private readonly IMediator _mediator;

    public InterviewSessionsController(ILogger<InterviewSessionsController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Get all interview sessions with pagination and filtering
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of interview sessions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetInterviewSessions([FromQuery] GetInterviewSessionsQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving interview sessions - Page: {Page}, PageSize: {PageSize}, UserId: {UserId}",
            query.Page, query.PageSize, query.UserId);

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Interview sessions retrieval failed: {Error}", result.FirstError);
            return ToActionResult(result);
        }

        return CreatePagedResponse(
            result.Value.Sessions,
            result.Value.TotalCount,
            query.Page,
            query.PageSize);
    }

    /// <summary>
    /// Get interview session by ID
    /// </summary>
    /// <param name="id">Interview session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Interview session details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetInterviewSessionById(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving interview session with ID: {SessionId}", id);

        var query = new GetInterviewSessionQuery { SessionId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Interview session retrieval failed for ID {SessionId}: {Error}", id, result.FirstError);
            return ToActionResult(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new interview session
    /// </summary>
    /// <param name="command">Create interview session command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created interview session details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateInterviewSession([FromBody] CreateInterviewSessionCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating interview session: {Title} for user: {UserId}", command.Title, command.UserId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Interview session creation failed: {Error}", result.FirstError);
            return ToActionResult(result);
        }

        _logger.LogInformation("Interview session created successfully with ID: {SessionId}", result.Value.Id);
        return CreatedAtAction(
            nameof(GetInterviewSessionById),
            new { id = result.Value.Id },
            result.Value);
    }

    /// <summary>
    /// Update an existing interview session
    /// </summary>
    /// <param name="id">Interview session ID</param>
    /// <param name="command">Update interview session command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated interview session details</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateInterviewSession(Guid id, [FromBody] UpdateInterviewSessionCommand command, CancellationToken cancellationToken = default)
    {
        if (id != command.SessionId)
        {
            _logger.LogWarning("Session ID mismatch: URL ID {UrlId} vs Command ID {CommandId}", id, command.SessionId);
            return BadRequest("Session ID in URL does not match the command");
        }

        _logger.LogInformation("Updating interview session with ID: {SessionId}", id);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Interview session update failed for ID {SessionId}: {Error}", id, result.FirstError);
            return ToActionResult(result);
        }

        _logger.LogInformation("Interview session updated successfully with ID: {SessionId}", id);
        return Ok(result.Value);
    }

    /// <summary>
    /// Delete an interview session
    /// </summary>
    /// <param name="id">Interview session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteInterviewSession(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting interview session with ID: {SessionId}", id);

        var command = new DeleteInterviewSessionCommand { SessionId = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Interview session deletion failed for ID {SessionId}: {Error}", id, result.FirstError);
            return ToActionResult(result);
        }

        _logger.LogInformation("Interview session deleted successfully with ID: {SessionId}", id);
        return NoContent();
    }

    /// <summary>
    /// Get interview sessions for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of user's interview sessions</returns>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetUserInterviewSessions(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving interview sessions for user: {UserId}", userId);

        var query = new GetInterviewSessionsQuery
        {
            UserId = userId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Interview sessions retrieval failed for user {UserId}: {Error}", userId, result.FirstError);
            return ToActionResult(result);
        }

        return CreatePagedResponse(
            result.Value.Sessions,
            result.Value.TotalCount,
            page,
            pageSize);
    }

    /// <summary>
    /// Start an interview session
    /// </summary>
    /// <param name="id">Interview session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> StartSession(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting interview session with ID: {SessionId}", id);

        var command = new StartInterviewSessionCommand { SessionId = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Interview session start failed for ID {SessionId}: {Error}", id, result.FirstError);
            return ToActionResult(result);
        }

        _logger.LogInformation("Interview session started successfully with ID: {SessionId}", id);
        return Ok();
    }

    /// <summary>
    /// Pause an interview session
    /// </summary>
    /// <param name="id">Interview session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    [HttpPost("{id:guid}/pause")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> PauseSession(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pausing interview session with ID: {SessionId}", id);

        var command = new PauseInterviewSessionCommand { SessionId = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Interview session pause failed for ID {SessionId}: {Error}", id, result.FirstError);
            return ToActionResult(result);
        }

        _logger.LogInformation("Interview session paused successfully with ID: {SessionId}", id);
        return Ok();
    }

    /// <summary>
    /// Resume an interview session
    /// </summary>
    /// <param name="id">Interview session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    [HttpPost("{id:guid}/resume")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ResumeSession(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Resuming interview session with ID: {SessionId}", id);

        var command = new ResumeInterviewSessionCommand { SessionId = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Interview session resume failed for ID {SessionId}: {Error}", id, result.FirstError);
            return ToActionResult(result);
        }

        _logger.LogInformation("Interview session resumed successfully with ID: {SessionId}", id);
        return Ok();
    }

    /// <summary>
    /// Complete an interview session
    /// </summary>
    /// <param name="id">Interview session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CompleteSession(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing interview session with ID: {SessionId}", id);

        var command = new CompleteInterviewSessionCommand { SessionId = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Interview session completion failed for ID {SessionId}: {Error}", id, result.FirstError);
            return ToActionResult(result);
        }

        _logger.LogInformation("Interview session completed successfully with ID: {SessionId}", id);
        return Ok();
    }
}
