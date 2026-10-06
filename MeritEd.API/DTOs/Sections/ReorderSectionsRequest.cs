using System.ComponentModel.DataAnnotations;

namespace MeritEd.API.DTOs.Sections;

public class ReorderSectionsRequest
{
    [Required]
    public List<Guid> OrderedIds { get; set; } = new();
}