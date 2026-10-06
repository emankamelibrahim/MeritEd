using System.ComponentModel.DataAnnotations;

namespace MeritEd.API.DTOs.Sections;

public class CreateSectionRequest
{
    [Required, MinLength(3), MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}