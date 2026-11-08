using AIPA.Domain.Enums;
using AIPA.Domain.Users.Entities;

namespace AIPA.Domain.Users.Specifications;

public static class UserSpecifications
{
    public static bool CanScheduleInterview(User user)
    {
        return user.Status == AccountStatus.Active;
    }

    public static bool CanUpdateProfile(User user)
    {
        return user.Status is AccountStatus.Active or AccountStatus.PendingVerification;
    }

    public static bool IsEligibleForInterviewTypes(User user, List<InterviewType> requestedTypes)
    {
        if (!CanScheduleInterview(user))
            return false;

        // Additional business rules based on user profile
        if (user.Profile.YearsOfExperience == null && requestedTypes.Contains(InterviewType.SystemDesign))
            return false;

        if (user.Profile.YearsOfExperience < 2 && requestedTypes.Contains(InterviewType.SystemDesign))
            return false;

        return true;
    }
}
