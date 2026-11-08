namespace AIPA.WebApi.Extensions;

/// <summary>
/// Extension methods for HttpContext
/// </summary>
public static class HttpContextExtensions
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CorrelationIdKey = "CorrelationId";

    /// <summary>
    /// Gets or creates a correlation ID for the request
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>The correlation ID</returns>
    public static string GetOrCreateCorrelationId(this HttpContext context)
    {
        // Check if correlation ID already exists in context
        if (context.Items.TryGetValue(CorrelationIdKey, out var existingId) && existingId is string correlationId)
        {
            return correlationId;
        }

        // Check if correlation ID exists in request headers
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValues))
        {
            correlationId = headerValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                context.Items[CorrelationIdKey] = correlationId;
                context.Response.Headers[CorrelationIdHeaderName] = correlationId;
                return correlationId;
            }
        }

        // Generate new correlation ID
        correlationId = Guid.NewGuid().ToString();
        context.Items[CorrelationIdKey] = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        return correlationId;
    }

    /// <summary>
    /// Gets the correlation ID from the context
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>The correlation ID or null if not found</returns>
    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items.TryGetValue(CorrelationIdKey, out var correlationId)
            ? correlationId as string
            : null;
    }

    /// <summary>
    /// Sets the correlation ID in the context
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="correlationId">The correlation ID to set</param>
    public static void SetCorrelationId(this HttpContext context, string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            throw new ArgumentException("Correlation ID cannot be null or empty", nameof(correlationId));

        context.Items[CorrelationIdKey] = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;
    }

    /// <summary>
    /// Gets the client IP address from the request
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>The client IP address</returns>
    public static string GetClientIpAddress(this HttpContext context)
    {
        // Check for forwarded headers first (for load balancers/proxies)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(ip))
                return ip;
        }

        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            var ip = realIp.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(ip))
                return ip;
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Gets the user agent from the request
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>The user agent string</returns>
    public static string GetUserAgent(this HttpContext context)
    {
        return context.Request.Headers.UserAgent.FirstOrDefault() ?? "Unknown";
    }
}
