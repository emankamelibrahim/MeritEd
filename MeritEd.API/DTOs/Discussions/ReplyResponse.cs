namespace MeritEd.API.DTOs.Discussions;

public class ReplyResponse
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsAcceptedAnswer { get; set; }
    public bool IsInstructorVerified { get; set; }
    public Guid? ParentReplyId { get; set; }
    public List<ReplyResponse> ChildReplies { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}