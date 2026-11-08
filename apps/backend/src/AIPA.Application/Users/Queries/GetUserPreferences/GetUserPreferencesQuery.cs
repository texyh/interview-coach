using AIPA.Application.DTOs.Users;
using MediatR;

namespace AIPA.Application.Users.Queries.GetUserPreferences;

/// <summary>
/// Query to get user preferences
/// </summary>
public record GetUserPreferencesQuery(
    Guid UserId
) : IRequest<UserPreferencesDto>;
