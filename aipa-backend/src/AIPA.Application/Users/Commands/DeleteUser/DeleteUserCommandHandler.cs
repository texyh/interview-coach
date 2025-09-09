using AIPA.Application.Common.Interfaces;
using AIPA.Application.Common.Models;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using MediatR;

namespace AIPA.Application.Users.Commands.DeleteUser;

/// <summary>
/// Handler for deleting a user
/// </summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.From(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result.Error($"User with ID {request.UserId} was not found");
        }

        // Soft delete by changing status to Deleted
        user.MarkAsDeleted();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
