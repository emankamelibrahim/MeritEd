using MeritEd.Core.Enums;

namespace MeritEd.API.DTOs.ContentItems;

public class ContentItemResponse
{
    public Guid Id { get; set; }
    public Guid SectionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }

    // Type-specific fields
    public string? Body { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public string? ExternalUrl { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public int? BaseXP { get; set; }
    public bool? AllowsRecovery { get; set; }
    public string? Questions { get; set; }
}