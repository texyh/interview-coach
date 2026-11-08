using System.Text.Json;
using AIPA.Domain.Common.Exceptions;
using AIPA.WebApi.Models;
using FluentValidation;

namespace AIPA.WebApi.Middleware;

/// <summary>
/// Global exception handling middleware that provides consistent error responses
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request processing");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        var apiResponse = new ApiResponse
        {
            Success = false,
            CorrelationId = correlationId
        };

        switch (exception)
        {
            case EntityNotFoundException notFoundEx:
                response.StatusCode = StatusCodes.Status404NotFound;
                apiResponse.Error = "EntityNotFound";
                apiResponse.Message = notFoundEx.Message;
                break;

            case DuplicateEntityException duplicateEx:
                response.StatusCode = StatusCodes.Status409Conflict;
                apiResponse.Error = "DuplicateEntity";
                apiResponse.Message = duplicateEx.Message;
                break;

            case BusinessRuleValidationException businessEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Error = "BusinessRuleViolation";
                apiResponse.Message = businessEx.Message;
                break;

            case DomainException domainEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Error = "DomainError";
                apiResponse.Message = domainEx.Message;
                break;

            case ValidationException validationEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Error = "ValidationFailed";
                apiResponse.Message = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage));
                break;

            case UnauthorizedAccessException:
                response.StatusCode = StatusCodes.Status401Unauthorized;
                apiResponse.Error = "Unauthorized";
                apiResponse.Message = "Authentication required";
                break;

            case KeyNotFoundException:
                response.StatusCode = StatusCodes.Status404NotFound;
                apiResponse.Error = "NotFound";
                apiResponse.Message = "The requested resource was not found";
                break;

            case ArgumentException argEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Error = "InvalidArgument";
                apiResponse.Message = argEx.Message;
                break;

            case NotImplementedException:
                response.StatusCode = StatusCodes.Status501NotImplemented;
                apiResponse.Error = "NotImplemented";
                apiResponse.Message = "This feature is not yet implemented";
                break;

            case TimeoutException:
                response.StatusCode = StatusCodes.Status408RequestTimeout;
                apiResponse.Error = "RequestTimeout";
                apiResponse.Message = "The request timed out";
                break;

            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                apiResponse.Error = "InternalServerError";
                apiResponse.Message = _environment.IsDevelopment()
                    ? exception.Message
                    : "An error occurred while processing your request";

                // Add stack trace in development
                if (_environment.IsDevelopment())
                {
                    apiResponse.Details = new
                    {
                        StackTrace = exception.StackTrace,
                        InnerException = exception.InnerException?.Message
                    };
                }
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await response.WriteAsync(jsonResponse);
    }
}
