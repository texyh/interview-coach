using AIPA.Application.Common.Interfaces;
using AIPA.Application.Common.Models;
using AIPA.Application.DTOs.Users;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace AIPA.Application.Users.Commands.UpdateUser;

/// <summary>
/// Handler for updating an existing user
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Get the existing user
        var userId = UserId.From(request.UserId);
        var existingUser = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (existingUser == null)
        {
            return Result<UserDto>.Error($"User with ID {request.UserId} was not found");
        }

        // Update user's full name
        var fullName = FullName.Create(request.FirstName, request.LastName);
        existingUser.UpdateFullName(fullName);

        // Update profile if provided
        if (request.Profile != null)
        {
            var userProfile = UserProfile.Create(
                jobTitle: null, // UserProfileDto doesn't have JobTitle, using null
                company: null,  // UserProfileDto doesn't have Company, using null
                yearsOfExperience: null, // UserProfileDto doesn't have YearsOfExperience, using null
                skills: request.Profile.Skills,
                preferredInterviewTypes: null); // UserProfileDto doesn't have PreferredInterviewTypes, using null

            existingUser.UpdateProfile(userProfile);
        }

        // Save changes
        await _userRepository.UpdateAsync(existingUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(existingUser);
        return Result<UserDto>.Success(userDto);
    }
}
