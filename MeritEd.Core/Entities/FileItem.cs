namespace MeritEd.Core.Entities;

public class FileItem : ContentItem
{
    public string? FileUrl { get; set; }
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
}