namespace AIPA.Domain.Enums;

public enum AccountStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    PendingVerification = 4,
    Deleted = 5
}

public enum InterviewType
{
    Technical = 1,
    Behavioral = 2,
    SystemDesign = 3,
    Coding = 4,
    Mock = 5
}

public enum SessionStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}

public enum QuestionDifficulty
{
    Easy = 1,
    Medium = 2,
    Hard = 3
}

public enum QuestionType
{
    MultipleChoice = 1,
    OpenEnded = 2,
    Coding = 3,
    SystemDesign = 4
}
