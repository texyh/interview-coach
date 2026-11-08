using AIPA.Application.DTOs.Users;

namespace AIPA.Application.Users.Queries.GetUsers;

/// <summary>
/// Response for GetUsers query
/// </summary>
public class GetUsersResponse
{
    /// <summary>
    /// Collection of users
    /// </summary>
    public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();

    /// <summary>
    /// Total count of users matching the query
    /// </summary>
    public int TotalCount { get; set; }
}
