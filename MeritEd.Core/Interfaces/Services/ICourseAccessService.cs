using MeritEd.Core.Enums;

namespace MeritEd.Core.Interfaces.Services;

public interface ICourseAccessService
{
    Task<CourseAccess> GetAccessAsync(Guid courseId, Guid userId);
    Task<CourseAccess> GetAccessForSectionAsync(Guid sectionId, Guid userId);
}