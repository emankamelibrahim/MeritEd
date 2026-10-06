using MeritEd.Core.Entities;

namespace MeritEd.Core.Interfaces.Services;

public interface ISectionService
{
    Task<Section> CreateSectionAsync(Guid courseId, Guid instructorId, string title);
    Task<IEnumerable<Section>> GetSectionsByCourseAsync(Guid courseId);
    Task<Section?> UpdateSectionAsync(Guid sectionId, Guid instructorId, string title, bool isPublished);
    Task<bool> DeleteSectionAsync(Guid sectionId, Guid instructorId);
    Task<bool> ReorderSectionsAsync(Guid courseId, Guid instructorId, List<Guid> orderedIds);
}