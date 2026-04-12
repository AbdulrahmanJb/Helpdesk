using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Application.Services;
using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;
using Xunit;

namespace Helpdesk.Tests;

public class AttachmentServiceTests
{
    [Fact]
    public async Task UploadAsync_ForAuthorizedRequester_CreatesAttachmentAndAuditEntry()
    {
        var service = CreateService(out var ticketRepository, out var attachmentRepository, out var fileStorageService, out var auditTrailService);
        ticketRepository.Tickets.Add(new Ticket
        {
            Id = 2,
            Title = "Attachment test",
            Description = "Ticket for testing file uploads.",
            RequesterId = 2,
            AgentId = 3
        });

        await using var stream = new MemoryStream([1, 2, 3, 4]);

        var result = await service.UploadAsync(2, 2, nameof(UserRole.Requester), "test.txt", "text/plain", stream, 4);

        Assert.NotNull(result);
        Assert.Single(attachmentRepository.Attachments);
        Assert.Single(fileStorageService.Files);
        Assert.Single(auditTrailService.Entries);
    }

    [Fact]
    public async Task DownloadAsync_DeniesRequesterForAnotherUsersTicket()
    {
        var service = CreateService(out var ticketRepository, out var attachmentRepository, out var fileStorageService, out _);
        ticketRepository.Tickets.Add(new Ticket
        {
            Id = 4,
            Title = "Private ticket",
            Description = "Attachment access test.",
            RequesterId = 99,
            AgentId = 3
        });
        attachmentRepository.Attachments.Add(new Attachment
        {
            Id = 1,
            TicketId = 4,
            UploadedByUserId = 3,
            OriginalFileName = "secret.txt",
            StoredFileName = "stored.txt",
            ContentType = "text/plain",
            FileSize = 5
        });
        fileStorageService.Files["stored.txt"] = [1, 2, 3];

        var result = await service.DownloadAsync(1, 2, nameof(UserRole.Requester));

        Assert.Null(result);
    }

    private static AttachmentService CreateService(
        out FakeTicketRepository ticketRepository,
        out FakeAttachmentRepository attachmentRepository,
        out FakeFileStorageService fileStorageService,
        out FakeAuditTrailService auditTrailService)
    {
        ticketRepository = new FakeTicketRepository();
        attachmentRepository = new FakeAttachmentRepository();
        fileStorageService = new FakeFileStorageService();
        auditTrailService = new FakeAuditTrailService();
        return new AttachmentService(attachmentRepository, ticketRepository, fileStorageService, auditTrailService);
    }

    private sealed class FakeTicketRepository : ITicketRepository
    {
        public List<Ticket> Tickets { get; } = [];

        public Task<Ticket> CreateAsync(Ticket ticket)
        {
            Tickets.Add(ticket);
            return Task.FromResult(ticket);
        }

        public Task<List<Ticket>> GetAllAsync() => Task.FromResult(Tickets.ToList());

        public Task<Ticket?> GetByIdAsync(int id) => Task.FromResult(Tickets.FirstOrDefault(ticket => ticket.Id == id));

        public Task UpdateAsync(Ticket ticket) => Task.CompletedTask;
    }

    private sealed class FakeAttachmentRepository : IAttachmentRepository
    {
        public List<Attachment> Attachments { get; } = [];

        public Task<Attachment> CreateAsync(Attachment attachment)
        {
            attachment.Id = Attachments.Count + 1;
            Attachments.Add(attachment);
            return Task.FromResult(attachment);
        }

        public Task<List<Attachment>> GetByTicketIdAsync(int ticketId)
        {
            return Task.FromResult(Attachments.Where(attachment => attachment.TicketId == ticketId).ToList());
        }

        public Task<Attachment?> GetByIdAsync(int id)
        {
            return Task.FromResult(Attachments.FirstOrDefault(attachment => attachment.Id == id));
        }

        public Task DeleteAsync(Attachment attachment)
        {
            Attachments.Remove(attachment);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public Dictionary<string, byte[]> Files { get; } = [];

        public async Task<string> SaveAsync(Stream content, string extension)
        {
            using var memoryStream = new MemoryStream();
            await content.CopyToAsync(memoryStream);
            var name = $"file{Files.Count + 1}{extension}";
            Files[name] = memoryStream.ToArray();
            return name;
        }

        public Task<byte[]?> ReadAsync(string storedFileName)
        {
            Files.TryGetValue(storedFileName, out var content);
            return Task.FromResult(content);
        }

        public Task DeleteAsync(string storedFileName)
        {
            Files.Remove(storedFileName);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuditTrailService : IAuditTrailService
    {
        public List<AuditTrailEntryResponseDto> Entries { get; } = [];

        public Task RecordAsync(int ticketId, int actorId, string action, string description)
        {
            Entries.Add(new AuditTrailEntryResponseDto
            {
                Id = Entries.Count + 1,
                TicketId = ticketId,
                ActorId = actorId,
                Action = action,
                Description = description,
                CreatedAt = DateTime.UtcNow
            });

            return Task.CompletedTask;
        }

        public Task<List<AuditTrailEntryResponseDto>?> GetTicketAuditTrailAsync(int ticketId, int userId, string role)
        {
            return Task.FromResult<List<AuditTrailEntryResponseDto>?>(Entries.Where(entry => entry.TicketId == ticketId).ToList());
        }
    }
}
