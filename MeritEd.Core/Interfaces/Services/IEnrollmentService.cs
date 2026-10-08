using MeritEd.Core.Entities;

namespace MeritEd.Core.Interfaces.Services;

public interface IEnrollmentService
{
    Task<(bool Success, string Error, Enrollment? Enrollment)> SelfEnrollAsync(
        Guid studentId, Guid courseId);

    Task<(bool Success, string Error, Enrollment? Enrollment)> ManualEnrollAsync(
        Guid instructorId, Guid courseId, Guid studentId);

    Task<(bool Success, string Error)> RemoveStudentAsync(
        Guid instructorId, Guid courseId, Guid studentId);

    Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(Guid courseId, Guid instructorId);

    Task<Enrollment?> GetEnrollmentAsync(Guid studentId, Guid courseId);
    Task<(bool Success, string Error)> LeaveCoursAsync(Guid studentId, Guid courseId);
}