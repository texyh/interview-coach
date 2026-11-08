using AIPA.Application.Common.Models;
using AIPA.Domain.Enums;
using MediatR;

namespace AIPA.Application.Users.Commands.DeleteUser;

/// <summary>
/// Command to delete a user
/// </summary>
public class DeleteUserCommand : IRequest<Result>
{
    /// <summary>
    /// User ID to delete
    /// </summary>
    public Guid UserId { get; set; }
}
