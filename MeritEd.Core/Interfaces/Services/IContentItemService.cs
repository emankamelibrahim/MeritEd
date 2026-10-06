using MeritEd.Core.Entities;
using MeritEd.Core.Enums;

namespace MeritEd.Core.Interfaces.Services;

public interface IContentItemService
{
    Task<ContentItem> CreateContentItemAsync(Guid sectionId, Guid instructorId,
        ContentItemType type, string title, string? body, string? fileUrl,
        long? fileSize, string? mimeType, string? externalUrl,
        string? description, DateTime? dueDate, int? baseXP,
        bool? allowsRecovery, string? questions);

    Task<IEnumerable<ContentItem>> GetContentItemsBySectionAsync(Guid sectionId, bool isInstructor);

    Task<ContentItem?> UpdatePublishStatusAsync(Guid itemId, Guid instructorId, bool isPublished);

    Task<bool> DeleteContentItemAsync(Guid itemId, Guid instructorId);

    Task<bool> ReorderContentItemsAsync(Guid sectionId, Guid instructorId, List<Guid> orderedIds);
}