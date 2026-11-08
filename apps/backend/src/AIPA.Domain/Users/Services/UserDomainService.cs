using AIPA.Domain.Common.Exceptions;
using AIPA.Domain.Users.Entities;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Services;

public class UserDomainService
{
    private readonly IUserRepository _userRepository;

    public UserDomainService(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<User> CreateUserAsync(
        Email email,
        FullName fullName,
        CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser != null)
            throw new DuplicateEntityException("User", "email", email.Value);

        // Create new user
        var user = User.Create(email, fullName);
        return user;
    }

    public async Task<bool> CanUpdateEmailAsync(
        UserId userId,
        Email newEmail,
        CancellationToken cancellationToken = default)
    {
        // Check if the new email is already taken by another user
        var existingUser = await _userRepository.GetByEmailAsync(newEmail, cancellationToken);
        return existingUser == null || existingUser.Id == userId;
    }

    public static bool IsValidProfileUpdate(UserProfile currentProfile, UserProfile newProfile)
    {
        // Business rules for profile updates
        if (newProfile.YearsOfExperience < 0)
            return false;

        if (newProfile.YearsOfExperience > 50)
            return false;

        return true;
    }

    public static List<string> ValidateUserProfile(UserProfile profile)
    {
        var errors = new List<string>();

        if (profile.YearsOfExperience < 0)
            errors.Add("Years of experience cannot be negative");

        if (profile.YearsOfExperience > 50)
            errors.Add("Years of experience cannot exceed 50 years");

        if (profile.Skills?.Count > 20)
            errors.Add("Cannot have more than 20 skills");

        if (profile.PreferredInterviewTypes?.Count > 5)
            errors.Add("Cannot have more than 5 preferred interview types");

        return errors;
    }
}
