namespace AIPA.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
