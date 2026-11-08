using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Users;
using MediatR;

namespace AIPA.Application.Users.Queries.GetUserByEmail;

/// <summary>
/// Query to get a user by email address
/// </summary>
public class GetUserByEmailQuery : IRequest<Result<UserDto>>
{
    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
