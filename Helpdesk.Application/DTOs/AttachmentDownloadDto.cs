namespace Helpdesk.Application.DTOs;

public class AttachmentDownloadDto
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public byte[] Content { get; set; } = [];
}
