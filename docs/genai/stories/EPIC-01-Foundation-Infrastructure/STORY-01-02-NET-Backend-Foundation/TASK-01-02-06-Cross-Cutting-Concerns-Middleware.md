# TASK-01-02-06: Cross-Cutting Concerns & Middleware

**Story**: STORY-01-02 .NET Backend Foundation with Clean Architecture
**Epic**: EPIC-01 Foundation Infrastructure & Development Environment
**Estimated Hours**: 7 hours
**Assignee**: Senior Backend Developer
**Sprint**: 2

## Task Description

Implement cross-cutting concerns including logging, exception handling, authentication middleware, request/response logging, performance monitoring, and health checks for the AI Interview Preparation Assistant.

## Acceptance Criteria

- [ ] Structured logging with Serilog configured
- [ ] Global exception handling middleware
- [ ] Request/response logging middleware
- [ ] Performance monitoring and metrics
- [ ] Health checks for database and external services
- [ ] CORS middleware properly configured
- [ ] Authentication middleware setup (JWT ready)
- [ ] Rate limiting middleware for API protection

## Technical Implementation

### Global Exception Handling Middleware

```csharp
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

        var apiResponse = new ApiResponse();

        switch (exception)
        {
            case DomainException domainEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Error = "Domain validation failed",
                    Message = domainEx.Message
                };
                break;

            case ValidationException validationEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Error = "Validation failed",
                    Message = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))
                };
                break;

            case UnauthorizedAccessException:
                response.StatusCode = StatusCodes.Status401Unauthorized;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Error = "Unauthorized",
                    Message = "Authentication required"
                };
                break;

            case KeyNotFoundException:
                response.StatusCode = StatusCodes.Status404NotFound;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Error = "Not found",
                    Message = "The requested resource was not found"
                };
                break;

            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = _environment.IsDevelopment() ? exception.Message : "An error occurred while processing your request"
                };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}
```

### Request/Response Logging Middleware

```csharp
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        // Log request
        await LogRequestAsync(context, correlationId);

        // Capture response
        var originalResponseBody = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log response
            await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);

            // Copy response back to original stream
            responseBodyStream.Position = 0;
            await responseBodyStream.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
        }
    }

    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        var request = context.Request;

        var requestBody = string.Empty;
        if (request.ContentLength > 0 && IsLoggableContentType(request.ContentType))
        {
            request.EnableBuffering();
            requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;
        }

        _logger.LogInformation(
            "HTTP Request: {Method} {Path} {QueryString} | CorrelationId: {CorrelationId} | Body: {RequestBody}",
            request.Method,
            request.Path,
            request.QueryString,
            correlationId,
            requestBody);
    }

    private async Task LogResponseAsync(HttpContext context, string correlationId, long elapsedMs)
    {
        var response = context.Response;

        var responseBody = string.Empty;
        if (response.Body.CanRead && IsLoggableContentType(response.ContentType))
        {
            response.Body.Position = 0;
            responseBody = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Position = 0;
        }

        _logger.LogInformation(
            "HTTP Response: {StatusCode} | CorrelationId: {CorrelationId} | Duration: {Duration}ms | Body: {ResponseBody}",
            response.StatusCode,
            correlationId,
            elapsedMs,
            responseBody);
    }

    private static bool IsLoggableContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        return contentType.Contains("application/json") ||
               contentType.Contains("text/plain") ||
               contentType.Contains("application/xml");
    }
}
```

### Performance Monitoring Middleware

```csharp
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

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Log slow requests
            if (elapsedMs > 1000) // Log requests taking more than 1 second
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} took {Duration}ms | StatusCode: {StatusCode}",
                    method,
                    path,
                    elapsedMs,
                    context.Response.StatusCode);
            }

            // Emit performance metrics
            if (_diagnosticSource.IsEnabled("AIPA.Performance"))
            {
                _diagnosticSource.Write("AIPA.Performance", new
                {
                    Method = method,
                    Path = path,
                    Duration = elapsedMs,
                    StatusCode = context.Response.StatusCode
                });
            }

            // Add performance headers
            context.Response.Headers.Add("X-Response-Time", $"{elapsedMs}ms");
        }
    }
}
```

### Health Checks Configuration

```csharp
public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<AipaDbContext>("database")
            .AddUrlGroup(
                uri: new Uri(configuration.GetConnectionString("AIService") ?? "http://localhost:3000/health"),
                name: "ai-service",
                failureStatus: HealthStatus.Degraded,
                timeout: TimeSpan.FromSeconds(5))
            .AddCheck<CustomHealthCheck>("custom-check");

        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(30);
            options.MaximumHistoryEntriesPerEndpoint(60);
            options.SetApiMaxActiveRequests(1);
            options.AddHealthCheckEndpoint("AIPA API", "/health");
        }).AddInMemoryStorage();

        return services;
    }
}

public class CustomHealthCheck : IHealthCheck
{
    private readonly ILogger<CustomHealthCheck> _logger;

    public CustomHealthCheck(ILogger<CustomHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check system resources, external dependencies, etc.
            var memoryUsage = GC.GetTotalMemory(false);
            var isHealthy = memoryUsage < 100_000_000; // 100MB threshold

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy($"Memory usage: {memoryUsage:N0} bytes"));
            }

            return Task.FromResult(HealthCheckResult.Degraded($"High memory usage: {memoryUsage:N0} bytes"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("An error occurred during health check", ex));
        }
    }
}
```

### Serilog Configuration

```csharp
public static class LoggingConfiguration
{
    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "AIPA")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/aipa-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
```

## Subtasks

1. **Exception Handling and Logging (2.5 hours)**

   - Implement global exception handling middleware
   - Configure Serilog with structured logging
   - Add correlation ID tracking for requests

2. **Request/Response Monitoring (2.5 hours)**

   - Implement request/response logging middleware
   - Add performance monitoring middleware
   - Configure diagnostic source for metrics

3. **Health Checks Implementation (1.5 hours)**

   - Set up health checks for database and external services
   - Implement health check UI for monitoring
   - Add custom health checks for system resources

4. **CORS and Security Middleware (0.5 hours)**
   - Configure CORS for development and production
   - Set up authentication middleware foundation
   - Add rate limiting configuration

## Definition of Done

- [ ] Global exception handling catches and logs all errors
- [ ] Structured logging provides comprehensive request tracing
- [ ] Performance monitoring identifies slow requests
- [ ] Health checks report accurate system status
- [ ] CORS is properly configured for all environments
- [ ] Authentication middleware is ready for JWT implementation
- [ ] All middleware is properly ordered in the pipeline
- [ ] Monitoring and logging work in both development and production

## Dependencies

- API layer implementation completed
- Infrastructure layer with database context ready
- Understanding of security and monitoring requirements

## Resources

- [ASP.NET Core Middleware Documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Serilog Configuration](https://serilog.net/)
- [Health Checks in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
