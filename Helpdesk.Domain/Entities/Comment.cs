namespace Helpdesk.Domain.Entities;

public class Comment
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public int AuthorId { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsInternal { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
