using AIPA.Application.Common.Interfaces;
using AIPA.Application.DTOs.Users;

namespace AIPA.Application.Users.Queries.GetUserById;

/// <summary>
/// Query to get a user by ID
/// </summary>
public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;
