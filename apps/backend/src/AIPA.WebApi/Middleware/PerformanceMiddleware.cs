using System.Diagnostics;

namespace AIPA.WebApi.Middleware;

/// <summary>
/// Middleware for monitoring performance and tracking slow requests
/// </summary>
public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;
    private readonly DiagnosticSource _diagnosticSource;

    public PerformanceMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMiddleware> logger,
        DiagnosticSource diagnosticSource)
    {
        _next = next;
        _logger = logger;
        _diagnosticSource = diagnosticSource;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var path = context.Request.Path.Value;
        var method = context.Request.Method;
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        // Track memory usage before request
        var memoryBefore = GC.GetTotalMemory(false);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var memoryAfter = GC.GetTotalMemory(false);
            var memoryUsed = memoryAfter - memoryBefore;

            // Log slow requests (threshold: 1000ms as per task spec)
            if (elapsedMs > 1000)
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} took {Duration}ms | StatusCode: {StatusCode} | CorrelationId: {CorrelationId} | MemoryUsed: {MemoryUsed} bytes",
                    method,
                    path,
                    elapsedMs,
                    context.Response.StatusCode,
                    correlationId,
                    memoryUsed);
            }

            // Log very slow requests with critical level
            if (elapsedMs > 5000)
            {
                _logger.LogCritical(
                    "Critical slow request: {Method} {Path} took {Duration}ms | StatusCode: {StatusCode} | CorrelationId: {CorrelationId}",
                    method,
                    path,
                    elapsedMs,
                    context.Response.StatusCode,
                    correlationId);
            }

            // Log high memory usage
            if (memoryUsed > 10_000_000) // 10MB threshold
            {
                _logger.LogWarning(
                    "High memory usage detected: {Method} {Path} used {MemoryUsed:N0} bytes | CorrelationId: {CorrelationId}",
                    method,
                    path,
                    memoryUsed,
                    correlationId);
            }

            // Emit performance metrics for monitoring tools
            if (_diagnosticSource.IsEnabled("AIPA.Performance"))
            {
                _diagnosticSource.Write("AIPA.Performance", new
                {
                    Method = method,
                    Path = path,
                    Duration = elapsedMs,
                    StatusCode = context.Response.StatusCode,
                    CorrelationId = correlationId,
                    MemoryUsed = memoryUsed,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            // Add performance headers for debugging
            context.Response.Headers["X-Response-Time"] = $"{elapsedMs}ms";
            context.Response.Headers["X-Memory-Used"] = $"{memoryUsed}";
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            // Log general performance info for debug level
            _logger.LogDebug(
                "Request completed: {Method} {Path} | Duration: {Duration}ms | Status: {StatusCode} | Memory: {MemoryUsed:N0} bytes | CorrelationId: {CorrelationId}",
                method,
                path,
                elapsedMs,
                context.Response.StatusCode,
                memoryUsed,
                correlationId);
        }
    }
}
