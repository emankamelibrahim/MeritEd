using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Services;

public class CourseAccessService : ICourseAccessService
{
    private readonly AppDbContext _context;

    public CourseAccessService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CourseAccess> GetAccessAsync(Guid courseId, Guid userId)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new { c.InstructorId, c.Status })
            .FirstOrDefaultAsync();

        if (course == null) return CourseAccess.None;
        if (course.InstructorId == userId) return CourseAccess.Owner;

        var enrolled = await _context.Enrollments.AnyAsync(e =>
            e.StudentId == userId && e.CourseId == courseId
            && e.Status == EnrollmentStatus.Active);

        if (enrolled) return CourseAccess.Enrolled;

        return course.Status == CourseStatus.Archived
            ? CourseAccess.None
            : CourseAccess.Preview;
    }

    public async Task<CourseAccess> GetAccessForSectionAsync(Guid sectionId, Guid userId)
    {
        var courseId = await _context.Sections
            .Where(s => s.Id == sectionId)
            .Select(s => (Guid?)s.CourseId)
            .FirstOrDefaultAsync();

        return courseId == null
            ? CourseAccess.None
            : await GetAccessAsync(courseId.Value, userId);
    }
}