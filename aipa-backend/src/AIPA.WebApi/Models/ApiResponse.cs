using System.Text.Json.Serialization;

namespace AIPA.WebApi.Models;

/// <summary>
/// Standard API response model for consistent error handling
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error code when operation fails
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for request tracking
    /// </summary>
    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Additional details for debugging (development only)
    /// </summary>
    [JsonPropertyName("details")]
    public object? Details { get; set; }
}

/// <summary>
/// Generic API response with data payload
/// </summary>
/// <typeparam name="T">Type of the data payload</typeparam>
public class ApiResponse<T> : ApiResponse
{
    /// <summary>
    /// Response data payload
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}
