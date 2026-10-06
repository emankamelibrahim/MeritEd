using MeritEd.Core.Entities;
using MeritEd.Core.Enums;

namespace MeritEd.Core.Interfaces.Services;

public interface ICourseService
{
    Task<Course> CreateCourseAsync(Guid instructorId, string title, string? description,
        DateOnly startDate, DateOnly? endDate);

    Task<Course?> GetCourseByIdAsync(Guid courseId);

    Task<IEnumerable<Course>> GetOpenCoursesAsync();

    Task<IEnumerable<Course>> GetInstructorCoursesAsync(Guid instructorId);

    Task<Course?> UpdateCourseAsync(Guid courseId, Guid instructorId, string title,
        string? description, DateOnly startDate, DateOnly? endDate,
        CourseStatus status, decimal recoveryMultiplier);

    Task<bool> DeleteCourseAsync(Guid courseId, Guid instructorId);

    Task<bool> IsInstructorOwnerAsync(Guid courseId, Guid instructorId);
}