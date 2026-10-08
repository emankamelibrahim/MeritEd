using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.Core.Entities;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Services;

public class ProgressService : IProgressService
{
    private readonly AppDbContext _context;

    public ProgressService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> GetStudentProgressAsync(Guid studentId, Guid courseId)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.StudentId == studentId
                && e.CourseId == courseId
                && e.Status == EnrollmentStatus.Active);
    }

    public async Task<IEnumerable<XPTransaction>> GetXPTransactionsAsync(
        Guid studentId, Guid courseId)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId
                && e.CourseId == courseId
                && e.Status == EnrollmentStatus.Active);

        if (enrollment == null)
            return Enumerable.Empty<XPTransaction>();

        return await _context.XPTransactions
            .Where(t => t.EnrollmentId == enrollment.Id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetStudentProgressForInstructorAsync(
        Guid instructorId, Guid courseId, Guid studentId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null) return null;

        return await _context.Enrollments
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.StudentId == studentId
                && e.CourseId == courseId
                && e.Status == EnrollmentStatus.Active);
    }
}