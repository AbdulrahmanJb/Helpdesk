namespace Helpdesk.Domain.Entities;

public class Attachment
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public int UploadedByUserId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
