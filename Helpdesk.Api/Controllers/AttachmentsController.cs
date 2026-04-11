using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Helpdesk.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tickets/{ticketId:int}/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAttachments(int ticketId)
    {
        if (!TryGetCurrentUser(out var userId, out var role))
            return Unauthorized();

        var attachments = await _attachmentService.GetTicketAttachmentsAsync(ticketId, userId, role);

        if (attachments == null)
            return NotFound();

        return Ok(attachments);
    }

    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadAttachment(int ticketId, IFormFile file)
    {
        if (!TryGetCurrentUser(out var userId, out var role))
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest("A non-empty file is required.");

        await using var stream = file.OpenReadStream();
        var attachment = await _attachmentService.UploadAsync(ticketId, userId, role, file.FileName, file.ContentType, stream, file.Length);

        if (attachment == null)
            return NotFound();

        return CreatedAtAction(nameof(GetAttachments), new { ticketId }, attachment);
    }

    [HttpGet("{attachmentId:int}/download")]
    public async Task<IActionResult> DownloadAttachment(int ticketId, int attachmentId)
    {
        if (!TryGetCurrentUser(out var userId, out var role))
            return Unauthorized();

        var attachment = await _attachmentService.DownloadAsync(attachmentId, userId, role);

        if (attachment == null)
            return NotFound();

        return File(attachment.Content, attachment.ContentType, attachment.FileName);
    }

    [HttpDelete("{attachmentId:int}")]
    public async Task<IActionResult> DeleteAttachment(int ticketId, int attachmentId)
    {
        if (!TryGetCurrentUser(out var userId, out var role))
            return Unauthorized();

        var deleted = await _attachmentService.DeleteAsync(attachmentId, userId, role);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private bool TryGetCurrentUser(out int userId, out string role)
    {
        userId = 0;
        role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId) && !string.IsNullOrWhiteSpace(role);
    }
}
