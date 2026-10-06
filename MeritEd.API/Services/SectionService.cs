using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.Core.Entities;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Services;

public class SectionService : ISectionService
{
    private readonly AppDbContext _context;

    public SectionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Section> CreateSectionAsync(Guid courseId, Guid instructorId, string title)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null)
            throw new UnauthorizedAccessException("Course not found or you do not own this course.");

        var maxOrder = await _context.Sections
            .Where(s => s.CourseId == courseId)
            .MaxAsync(s => (int?)s.OrderIndex) ?? 0;

        var section = new Section
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title,
            OrderIndex = maxOrder + 1,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Sections.Add(section);
        await _context.SaveChangesAsync();
        return section;
    }

    public async Task<IEnumerable<Section>> GetSectionsByCourseAsync(Guid courseId)
    {
        return await _context.Sections
            .Include(s => s.ContentItems)
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.OrderIndex)
            .ToListAsync();
    }

    public async Task<Section?> UpdateSectionAsync(Guid sectionId, Guid instructorId,
        string title, bool isPublished)
    {
        var section = await _context.Sections
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sectionId && s.Course.InstructorId == instructorId);

        if (section == null) return null;

        section.Title = title;
        section.IsPublished = isPublished;
        section.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return section;
    }

    public async Task<bool> DeleteSectionAsync(Guid sectionId, Guid instructorId)
    {
        var section = await _context.Sections
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sectionId && s.Course.InstructorId == instructorId);

        if (section == null) return false;

        _context.Sections.Remove(section);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderSectionsAsync(Guid courseId, Guid instructorId, List<Guid> orderedIds)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null) return false;

        var sections = await _context.Sections
            .Where(s => s.CourseId == courseId)
            .ToListAsync();

        for (int i = 0; i < orderedIds.Count; i++)
        {
            var section = sections.FirstOrDefault(s => s.Id == orderedIds[i]);
            if (section != null)
            {
                section.OrderIndex = i + 1;
                section.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }
}