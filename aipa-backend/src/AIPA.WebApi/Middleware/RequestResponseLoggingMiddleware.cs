using System.Diagnostics;
using System.Text;
using AIPA.WebApi.Extensions;

namespace AIPA.WebApi.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with correlation tracking
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly HashSet<string> _excludedPaths;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        // Paths to exclude from detailed logging
        _excludedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/health",
            "/healthz",
            "/ready",
            "/metrics",
            "/favicon.ico",
            "/robots.txt"
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.GetOrCreateCorrelationId();

        // Skip logging for excluded paths
        if (_excludedPaths.Contains(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        // Log incoming request
        await LogRequestAsync(context, correlationId);

        // Capture original response stream
        var originalResponseStream = context.Response.Body;
        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log outgoing response
            await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);

            // Copy response back to original stream
            responseStream.Seek(0, SeekOrigin.Begin);
            await responseStream.CopyToAsync(originalResponseStream);
        }
    }

    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        try
        {
            var request = context.Request;
            var requestBody = string.Empty;

            // Read request body for POST/PUT requests (if applicable)
            if (request.ContentLength > 0 &&
                (request.Method == "POST" || request.Method == "PUT" || request.Method == "PATCH") &&
                request.ContentType?.Contains("application/json") == true)
            {
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                var bytesRead = await request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length));
                requestBody = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                request.Body.Seek(0, SeekOrigin.Begin);
            }

            _logger.LogInformation(
                "HTTP Request {Method} {Path} started. CorrelationId: {CorrelationId}, ContentType: {ContentType}, ContentLength: {ContentLength}, UserAgent: {UserAgent}, RemoteIP: {RemoteIP}",
                request.Method,
                $"{request.Path}{request.QueryString}",
                correlationId,
                request.ContentType ?? "N/A",
                request.ContentLength ?? 0,
                request.Headers.UserAgent.ToString(),
                context.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            // Log request body for debugging (be careful with sensitive data)
            if (!string.IsNullOrEmpty(requestBody) && requestBody.Length < 5000)
            {
                _logger.LogDebug(
                    "Request Body for {CorrelationId}: {RequestBody}",
                    correlationId,
                    requestBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error logging request for CorrelationId: {CorrelationId}",
                correlationId);
        }
    }

    private async Task LogResponseAsync(HttpContext context, string correlationId, long elapsedMilliseconds)
    {
        try
        {
            var response = context.Response;
            var responseBody = string.Empty;

            // Read response body
            if (response.Body.CanSeek)
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
                responseBody = await reader.ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);
            }

            var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

            _logger.Log(logLevel,
                "HTTP Response {StatusCode} for {Method} {Path} completed in {ElapsedMs}ms. CorrelationId: {CorrelationId}, ContentType: {ContentType}, ContentLength: {ContentLength}",
                response.StatusCode,
                context.Request.Method,
                $"{context.Request.Path}{context.Request.QueryString}",
                elapsedMilliseconds,
                correlationId,
                response.ContentType ?? "N/A",
                response.ContentLength ?? responseBody.Length);

            // Log response body for debugging (be careful with sensitive data)
            if (!string.IsNullOrEmpty(responseBody) && responseBody.Length < 5000)
            {
                _logger.LogDebug(
                    "Response Body for {CorrelationId}: {ResponseBody}",
                    correlationId,
                    responseBody);
            }

            // Log performance warnings
            if (elapsedMilliseconds > 5000)
            {
                _logger.LogWarning(
                    "Slow response detected: {Method} {Path} took {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    $"{context.Request.Path}{context.Request.QueryString}",
                    elapsedMilliseconds,
                    correlationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error logging response for CorrelationId: {CorrelationId}",
                correlationId);
        }
    }
}
