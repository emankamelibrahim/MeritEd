using MeritEd.Core.Entities;

namespace MeritEd.Core.Interfaces.Services;

public interface IDiscussionService
{
    Task<(bool Success, string Error, DiscussionThread? Thread)> CreateThreadAsync(
        Guid authorId, Guid courseId, string title, string body, string? tags);

    Task<IEnumerable<DiscussionThread>> GetThreadsAsync(Guid courseId, Guid userId);

    Task<DiscussionThread?> GetThreadByIdAsync(Guid threadId, Guid userId);

    Task<(bool Success, string Error, DiscussionReply? Reply)> CreateReplyAsync(
        Guid authorId, Guid threadId, string body, Guid? parentReplyId);

    Task<(bool Success, string Error)> MarkAcceptedAnswerAsync(
        Guid threadId, Guid replyId, Guid userId);

    Task<(bool Success, string Error)> VerifyReplyAsync(
        Guid threadId, Guid replyId, Guid instructorId);

    Task<(bool Success, string Error)> DeleteThreadAsync(Guid threadId, Guid userId);
}