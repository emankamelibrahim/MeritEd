using System.ComponentModel.DataAnnotations;

namespace MeritEd.API.DTOs.Discussions;

public class CreateThreadRequest
{
    [Required, MinLength(3), MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public string? Tags { get; set; }
}