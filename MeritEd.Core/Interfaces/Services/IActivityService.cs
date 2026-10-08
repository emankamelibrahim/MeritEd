using MeritEd.Core.Entities;

namespace MeritEd.Core.Interfaces.Services;

public interface IActivityService
{
    Task<(bool Success, string Error, Activity? Activity)> SubmitAsync(
        Guid studentId, Guid contentItemId);

    Task<(bool Success, string Error, Activity? Activity)> ApproveAsync(
        Guid instructorId, Guid activityId);

    Task<(bool Success, string Error, Activity? Activity)> RejectAsync(
        Guid instructorId, Guid activityId);

    Task<IEnumerable<Activity>> GetCourseActivitiesAsync(Guid courseId, Guid instructorId);

    Task<IEnumerable<Activity>> GetItemActivitiesAsync(Guid contentItemId, Guid instructorId);
}