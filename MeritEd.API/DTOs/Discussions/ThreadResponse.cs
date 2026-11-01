namespace MeritEd.API.DTOs.Discussions;

public class ThreadResponse
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public bool IsClosed { get; set; }
    public int ReplyCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReplyResponse> Replies { get; set; } = new();
}