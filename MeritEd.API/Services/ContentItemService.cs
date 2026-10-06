using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.Core.Entities;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;
using System.Text.Json;

namespace MeritEd.API.Services;

public class ContentItemService : IContentItemService
{
    private readonly AppDbContext _context;

    public ContentItemService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ContentItem> CreateContentItemAsync(Guid sectionId, Guid instructorId,
        ContentItemType type, string title, string? body, string? fileUrl,
        long? fileSize, string? mimeType, string? externalUrl,
        string? description, DateTime? dueDate, int? baseXP,
        bool? allowsRecovery, string? questions)
    {
        var section = await _context.Sections
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sectionId && s.Course.InstructorId == instructorId);

        if (section == null)
            throw new UnauthorizedAccessException("Section not found or you do not own this course.");

        var maxOrder = await _context.ContentItems
            .Where(c => c.SectionId == sectionId)
            .MaxAsync(c => (int?)c.OrderIndex) ?? 0;

        ContentItem item = type switch
        {
            ContentItemType.Lecture => new Lecture
            {
                Body = body
            },
            ContentItemType.File => new FileItem
            {
                FileUrl = fileUrl,
                FileSize = fileSize,
                MimeType = mimeType
            },
            ContentItemType.Link => new LinkItem
            {
                ExternalUrl = externalUrl,
                Description = description
            },
            ContentItemType.Assignment => new Assignment
            {
                Description = description,
                DueDate = dueDate,
                BaseXP = baseXP ?? 0,
                AllowsRecovery = allowsRecovery ?? false
            },
            ContentItemType.Quiz => new Quiz
            {
                DueDate = dueDate,
                BaseXP = baseXP ?? 0,
                AllowsRecovery = allowsRecovery ?? false,
                Questions = questions != null
                    ? JsonDocument.Parse(questions)
                    : null
            },
            _ => throw new ArgumentException("Invalid content item type.")
        };

        item.Id = Guid.NewGuid();
        item.SectionId = sectionId;
        item.Title = title;
        item.OrderIndex = maxOrder + 1;
        item.IsPublished = false;
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        _context.ContentItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<IEnumerable<ContentItem>> GetContentItemsBySectionAsync(
        Guid sectionId, bool isInstructor)
    {
        var query = _context.ContentItems
            .Where(c => c.SectionId == sectionId);

        if (!isInstructor)
            query = query.Where(c => c.IsPublished);

        return await query
            .OrderBy(c => c.OrderIndex)
            .ToListAsync();
    }

    public async Task<ContentItem?> UpdatePublishStatusAsync(
        Guid itemId, Guid instructorId, bool isPublished)
    {
        var item = await _context.ContentItems
            .Include(c => c.Section)
                .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(c => c.Id == itemId &&
                c.Section.Course.InstructorId == instructorId);

        if (item == null) return null;

        item.IsPublished = isPublished;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteContentItemAsync(Guid itemId, Guid instructorId)
    {
        var item = await _context.ContentItems
            .Include(c => c.Section)
                .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(c => c.Id == itemId &&
                c.Section.Course.InstructorId == instructorId);

        if (item == null) return false;

        _context.ContentItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderContentItemsAsync(
        Guid sectionId, Guid instructorId, List<Guid> orderedIds)
    {
        var section = await _context.Sections
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sectionId &&
                s.Course.InstructorId == instructorId);

        if (section == null) return false;

        var items = await _context.ContentItems
            .Where(c => c.SectionId == sectionId)
            .ToListAsync();

        for (int i = 0; i < orderedIds.Count; i++)
        {
            var item = items.FirstOrDefault(c => c.Id == orderedIds[i]);
            if (item != null)
            {
                item.OrderIndex = i + 1;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }
}