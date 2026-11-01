namespace MeritEd.Core.Entities;

public class DiscussionReply
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public DiscussionThread Thread { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public string Body { get; set; } = string.Empty;
    public bool IsAcceptedAnswer { get; set; }
    public bool IsInstructorVerified { get; set; }
    public Guid? ParentReplyId { get; set; }
    public DiscussionReply? ParentReply { get; set; }
    public ICollection<DiscussionReply> ChildReplies { get; set; } = new List<DiscussionReply>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}