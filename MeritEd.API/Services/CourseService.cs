using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.Core.Entities;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _context;

    public CourseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Course> CreateCourseAsync(Guid instructorId, string title,
     string? description, DateOnly startDate, DateOnly? endDate)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            InstructorId = instructorId,
            Title = title,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Status = CourseStatus.Open,
            RecoveryMultiplier = 0.70m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        _context.Entry(course).State = EntityState.Detached;

        return (await _context.Courses
            .AsNoTracking()
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == course.Id))!;
    }
    public async Task<Course?> GetCourseByIdAsync(Guid courseId)
    {
        return await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Sections)
                .ThenInclude(s => s.ContentItems)
            .FirstOrDefaultAsync(c => c.Id == courseId);
    }

    public async Task<IEnumerable<Course>> GetOpenCoursesAsync()
    {
        return await _context.Courses
            .Include(c => c.Instructor)
            .Where(c => c.Status == CourseStatus.Open)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetInstructorCoursesAsync(Guid instructorId)
    {
        return await _context.Courses
            .Include(c => c.Sections)
            .Where(c => c.InstructorId == instructorId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Course?> UpdateCourseAsync(Guid courseId, Guid instructorId,
        string title, string? description, DateOnly startDate, DateOnly? endDate,
        CourseStatus status, decimal recoveryMultiplier)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null) return null;

        course.Title = title;
        course.Description = description;
        course.StartDate = startDate;
        course.EndDate = endDate;
        course.Status = status;
        course.RecoveryMultiplier = recoveryMultiplier;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _context.Entry(course).State = EntityState.Detached;

        return await _context.Courses
            .AsNoTracking()
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == courseId);
    }
    public async Task<bool> DeleteCourseAsync(Guid courseId, Guid instructorId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null) return false;

        course.Status = CourseStatus.Archived;
        course.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsInstructorOwnerAsync(Guid courseId, Guid instructorId)
    {
        return await _context.Courses
            .AnyAsync(c => c.Id == courseId && c.InstructorId == instructorId);
    }
}