using MeritEd.Core.Entities;
using MeritEd.Core.Enums;

namespace MeritEd.Core.Interfaces.Services;

public interface IProgressService
{
    Task<Enrollment?> GetStudentProgressAsync(Guid studentId, Guid courseId);
    Task<IEnumerable<XPTransaction>> GetXPTransactionsAsync(Guid studentId, Guid courseId);
    Task<Enrollment?> GetStudentProgressForInstructorAsync(Guid instructorId, Guid courseId, Guid studentId);
}