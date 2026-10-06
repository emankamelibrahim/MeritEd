using System.ComponentModel.DataAnnotations;
using MeritEd.Core.Enums;

namespace MeritEd.API.DTOs.ContentItems;

public class CreateContentItemRequest
{
    [Required]
    public ContentItemType Type { get; set; }

    [Required, MinLength(3), MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    // Lecture
    public string? Body { get; set; }

    // File
    public string? FileUrl { get; set; }
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }

    // Link
    public string? ExternalUrl { get; set; }

    // Link and Assignment
    public string? Description { get; set; }

    // Assignment and Quiz
    public DateTime? DueDate { get; set; }
    public int? BaseXP { get; set; }
    public bool? AllowsRecovery { get; set; }

    // Quiz
    public string? Questions { get; set; }
}