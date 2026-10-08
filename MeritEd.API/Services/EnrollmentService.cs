using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.Core.Entities;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _context;

    public EnrollmentService(AppDbContext context)
    {
        _context = context;
    }

    private async Task<Enrollment> ReloadWithStudentAsync(Guid enrollmentId)
    {
        return (await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId))!;
    }

    public async Task<(bool Success, string Error, Enrollment? Enrollment)> SelfEnrollAsync(
        Guid studentId, Guid courseId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return (false, "Course not found.", null);

        if (course.Status != CourseStatus.Open)
            return (false, "Course is not open for enrollment.", null);

        var existing = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        if (existing != null)
        {
            if (existing.Status == EnrollmentStatus.Active)
                return (false, "Already enrolled in this course.", null);

            existing.Status = EnrollmentStatus.Active;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, string.Empty, await ReloadWithStudentAsync(existing.Id));
        }

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CourseId = courseId,
            Status = EnrollmentStatus.Active,
            TotalXP = 0,
            EnrolledAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return (true, string.Empty, await ReloadWithStudentAsync(enrollment.Id));
    }

    public async Task<(bool Success, string Error, Enrollment? Enrollment)> ManualEnrollAsync(
        Guid instructorId, Guid courseId, Guid studentId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null)
            return (false, "Course not found or you do not own this course.", null);

        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == UserRole.Student);

        if (student == null)
            return (false, "Student not found.", null);

        var existing = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        if (existing != null)
        {
            if (existing.Status == EnrollmentStatus.Active)
                return (false, "Student is already enrolled.", null);

            existing.Status = EnrollmentStatus.Active;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, string.Empty, await ReloadWithStudentAsync(existing.Id));
        }

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CourseId = courseId,
            Status = EnrollmentStatus.Active,
            TotalXP = 0,
            EnrolledAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return (true, string.Empty, await ReloadWithStudentAsync(enrollment.Id));
    }

    public async Task<(bool Success, string Error)> RemoveStudentAsync(
        Guid instructorId, Guid courseId, Guid studentId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null)
            return (false, "Course not found or you do not own this course.");

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId
                && e.Status == EnrollmentStatus.Active);

        if (enrollment == null)
            return (false, "Student is not enrolled in this course.");

        enrollment.Status = EnrollmentStatus.Removed;
        enrollment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(
        Guid courseId, Guid instructorId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null)
            return Enumerable.Empty<Enrollment>();

        return await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active)
            .OrderBy(e => e.EnrolledAt)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetEnrollmentAsync(Guid studentId, Guid courseId)
    {
        return await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId
                && e.CourseId == courseId
                && e.Status == EnrollmentStatus.Active);
    }
    public async Task<(bool Success, string Error)> LeaveCoursAsync(Guid studentId, Guid courseId)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId
                && e.Status == EnrollmentStatus.Active);

        if (enrollment == null)
            return (false, "You are not enrolled in this course.");

        enrollment.Status = EnrollmentStatus.Removed;
        enrollment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }
}