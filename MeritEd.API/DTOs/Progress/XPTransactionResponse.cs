namespace MeritEd.API.DTOs.Progress;

public class XPTransactionResponse
{
    public Guid Id { get; set; }
    public int Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public Guid? SourceId { get; set; }
    public DateTime CreatedAt { get; set; }
}