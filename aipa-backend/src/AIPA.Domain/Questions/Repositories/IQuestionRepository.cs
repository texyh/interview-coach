using AIPA.Domain.Enums;
using AIPA.Domain.Questions.Entities;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Questions.Repositories;

public interface IQuestionRepository
{
    Task<InterviewQuestion?> GetByIdAsync(QuestionId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewQuestion>> GetByTypeAsync(QuestionType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewQuestion>> GetByDifficultyAsync(QuestionDifficulty difficulty, CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewQuestion>> GetByTagsAsync(List<string> tags, CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewQuestion>> GetRandomQuestionsAsync(
        int count,
        QuestionType? type = null,
        QuestionDifficulty? difficulty = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(QuestionId id, CancellationToken cancellationToken = default);
    Task AddAsync(InterviewQuestion question, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<InterviewQuestion> questions, CancellationToken cancellationToken = default);
    Task UpdateAsync(InterviewQuestion question, CancellationToken cancellationToken = default);
    Task DeleteAsync(QuestionId id, CancellationToken cancellationToken = default);
}
