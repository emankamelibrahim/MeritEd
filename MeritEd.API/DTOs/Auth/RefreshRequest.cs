using System.ComponentModel.DataAnnotations;

namespace MeritEd.API.DTOs.Auth;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}