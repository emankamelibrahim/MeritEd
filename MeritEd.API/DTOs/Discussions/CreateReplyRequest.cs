using System.ComponentModel.DataAnnotations;

namespace MeritEd.API.DTOs.Discussions;

public class CreateReplyRequest
{
    [Required]
    public string Body { get; set; } = string.Empty;
    public Guid? ParentReplyId { get; set; }
}