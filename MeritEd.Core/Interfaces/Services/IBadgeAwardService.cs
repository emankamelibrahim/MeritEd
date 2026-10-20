using MeritEd.Core.Entities;

namespace MeritEd.Core.Interfaces.Services;

public interface IBadgeAwardService
{
    Task EvaluateAndAwardBadgesAsync(Enrollment enrollment, bool isFirstSubmission);
    Task<IEnumerable<StudentBadge>> GetStudentBadgesAsync(Guid enrollmentId);
}