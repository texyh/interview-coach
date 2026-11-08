namespace AIPA.Application.DTOs.Users;

/// <summary>
/// User data transfer object
/// </summary>
public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// User profile data transfer object
/// </summary>
public record UserProfileDto
{
    public string? Bio { get; init; }
    public string? PhoneNumber { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? GitHubUrl { get; init; }
    public List<string> Skills { get; init; } = new();
    public List<string> Interests { get; init; } = new();
}

/// <summary>
/// Simplified user data transfer object for lists
/// </summary>
public record UserSummaryDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
