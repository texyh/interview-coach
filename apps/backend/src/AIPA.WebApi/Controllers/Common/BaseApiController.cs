using Microsoft.AspNetCore.Mvc;
using AIPA.Application.Common.Models;

namespace AIPA.WebApi.Controllers.Common;

/// <summary>
/// Base controller providing common functionality for API controllers.
/// Handles Result pattern conversion to HTTP responses.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Converts a Result to an appropriate ActionResult.
    /// </summary>
    /// <param name="result">The result to convert</param>
    /// <returns>ActionResult based on result status</returns>
    protected IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.FirstError);
    }

    /// <summary>
    /// Converts a Result with value to an appropriate ActionResult.
    /// </summary>
    /// <typeparam name="T">The type of the result value</typeparam>
    /// <param name="result">The result to convert</param>
    /// <returns>IActionResult with value based on result status</returns>
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return ToErrorResult(result.FirstError);
    }

    /// <summary>
    /// Converts an error message to the appropriate HTTP status code and response.
    /// </summary>
    /// <param name="errorMessage">The error message to convert</param>
    /// <returns>ActionResult with appropriate status code</returns>
    protected IActionResult ToErrorResult(string errorMessage)
    {
        // Default to BadRequest for validation errors, can be enhanced based on error message patterns
        var statusCode = DetermineStatusCodeFromError(errorMessage);

        return StatusCode(statusCode, new
        {
            error = "Error",
            message = errorMessage
        });
    }

    /// <summary>
    /// Determines the appropriate HTTP status code based on the error message.
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <returns>HTTP status code</returns>
    private int DetermineStatusCodeFromError(string errorMessage)
    {
        var lowerError = errorMessage.ToLower();

        if (lowerError.Contains("not found") || lowerError.Contains("does not exist"))
            return 404; // Not Found

        if (lowerError.Contains("already exists") || lowerError.Contains("duplicate") || lowerError.Contains("conflict"))
            return 409; // Conflict

        if (lowerError.Contains("validation") || lowerError.Contains("invalid") || lowerError.Contains("required"))
            return 400; // Bad Request

        if (lowerError.Contains("unauthorized") || lowerError.Contains("access denied"))
            return 401; // Unauthorized

        if (lowerError.Contains("forbidden"))
            return 403; // Forbidden

        return 400; // Default to Bad Request
    }

    /// <summary>
    /// Creates a success response with data.
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="data">The data to return</param>
    /// <returns>Ok result with data</returns>
    protected IActionResult CreateResponse<T>(T data)
    {
        return Ok(data);
    }

    /// <summary>
    /// Creates a paginated response.
    /// </summary>
    /// <typeparam name="T">The type of the items</typeparam>
    /// <param name="items">The items</param>
    /// <param name="totalCount">Total count of items</param>
    /// <param name="pageNumber">Current page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Ok result with paginated data</returns>
    protected IActionResult CreatePagedResponse<T>(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        var response = new
        {
            Data = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasNextPage = pageNumber < Math.Ceiling((double)totalCount / pageSize),
            HasPreviousPage = pageNumber > 1
        };

        return Ok(response);
    }

    /// <summary>
    /// Creates a 201 Created response.
    /// </summary>
    /// <typeparam name="T">The type of the created resource</typeparam>
    /// <param name="actionName">The action name for the Location header</param>
    /// <param name="routeValues">Route values for the Location header</param>
    /// <param name="value">The created resource</param>
    /// <returns>Created result</returns>
    protected IActionResult CreateCreatedResponse<T>(string actionName, object routeValues, T value)
    {
        return CreatedAtAction(actionName, routeValues, value);
    }
}
