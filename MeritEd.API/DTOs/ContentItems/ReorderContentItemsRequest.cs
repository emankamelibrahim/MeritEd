using System.ComponentModel.DataAnnotations;

namespace MeritEd.API.DTOs.ContentItems;

public class ReorderContentItemsRequest
{
    [Required]
    public List<Guid> OrderedIds { get; set; } = new();
}