using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Users;
using MediatR;

namespace AIPA.Application.Users.Queries.GetUser;

/// <summary>
/// Query to get a user by ID
/// </summary>
public class GetUserQuery : IRequest<Result<UserDto>>
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }
}
