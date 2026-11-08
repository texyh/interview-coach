namespace AIPA.Domain.ValueObjects;

public record InterviewSessionId
{
    public Guid Value { get; }

    private InterviewSessionId(Guid value)
    {
        Value = value;
    }

    public static InterviewSessionId New() => new(Guid.NewGuid());

    public static InterviewSessionId From(Guid value) => new(value);

    public static implicit operator Guid(InterviewSessionId sessionId) => sessionId.Value;

    public override string ToString() => Value.ToString();
}
