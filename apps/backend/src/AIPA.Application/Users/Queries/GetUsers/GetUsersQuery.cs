using AIPA.Application.Common.Interfaces;
using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Users;

namespace AIPA.Application.Users.Queries.GetUsers;

/// <summary>
/// Query to get a paginated list of users
/// </summary>
public record GetUsersQuery(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? Status = null) : IQuery<GetUsersResponse>;
