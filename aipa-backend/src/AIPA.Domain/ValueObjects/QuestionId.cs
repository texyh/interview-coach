namespace AIPA.Domain.ValueObjects;

public record QuestionId
{
    public Guid Value { get; }

    private QuestionId(Guid value)
    {
        Value = value;
    }

    public static QuestionId New() => new(Guid.NewGuid());

    public static QuestionId From(Guid value) => new(value);

    public static implicit operator Guid(QuestionId questionId) => questionId.Value;

    public override string ToString() => Value.ToString();
}
