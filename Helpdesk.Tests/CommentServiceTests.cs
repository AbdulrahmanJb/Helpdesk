using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Application.Services;
using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;
using Xunit;

namespace Helpdesk.Tests;

public class CommentServiceTests
{
    [Fact]
    public async Task CreateCommentAsync_PublicComment_NotifiesRequesterWhenCommentComesFromAgent()
    {
        var service = CreateService(out var ticketRepository, out _, out var auditTrailService, out var notificationService);
        ticketRepository.Tickets.Add(new Ticket
        {
            Id = 2,
            Title = "Printer issue",
            Description = "Printer not working in the office.",
            RequesterId = 2,
            AgentId = 3
        });

        var result = await service.CreateCommentAsync(
            ticketId: 2,
            new CreateCommentDto { Content = "I am checking the printer now.", IsInternal = false },
            userId: 3,
            role: nameof(UserRole.Agent));

        Assert.Equal(CreateCommentResult.Success, result.Result);
        Assert.Single(auditTrailService.Entries);
        Assert.Single(notificationService.Notifications);
        Assert.Equal(2, notificationService.Notifications[0].UserId);
    }

    [Fact]
    public async Task CreateCommentAsync_RequesterCannotCreateInternalComment()
    {
        var service = CreateService(out var ticketRepository, out _, out _, out _);
        ticketRepository.Tickets.Add(new Ticket
        {
            Id = 4,
            Title = "Email issue",
            Description = "Requester ticket for internal comment rule.",
            RequesterId = 2,
            AgentId = 3
        });

        var result = await service.CreateCommentAsync(
            ticketId: 4,
            new CreateCommentDto { Content = "Trying to add internal note.", IsInternal = true },
            userId: 2,
            role: nameof(UserRole.Requester));

        Assert.Equal(CreateCommentResult.Forbidden, result.Result);
    }

    [Fact]
    public async Task GetTicketCommentsAsync_RequesterCannotSeeInternalComments()
    {
        var service = CreateService(out var ticketRepository, out var commentRepository, out _, out _);
        ticketRepository.Tickets.Add(new Ticket
        {
            Id = 8,
            Title = "VPN issue",
            Description = "Requester should only see public comments.",
            RequesterId = 2,
            AgentId = 3
        });

        commentRepository.Comments.AddRange(
            new Comment { Id = 1, TicketId = 8, AuthorId = 3, Content = "Public update", IsInternal = false },
            new Comment { Id = 2, TicketId = 8, AuthorId = 3, Content = "Internal note", IsInternal = true });

        var result = await service.GetTicketCommentsAsync(8, 2, nameof(UserRole.Requester));

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.False(result[0].IsInternal);
    }

    private static CommentService CreateService(
        out FakeTicketRepository ticketRepository,
        out FakeCommentRepository commentRepository,
        out FakeAuditTrailService auditTrailService,
        out FakeNotificationService notificationService)
    {
        ticketRepository = new FakeTicketRepository();
        commentRepository = new FakeCommentRepository();
        auditTrailService = new FakeAuditTrailService();
        notificationService = new FakeNotificationService();
        return new CommentService(commentRepository, ticketRepository, auditTrailService, notificationService);
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

    private sealed class FakeCommentRepository : ICommentRepository
    {
        public List<Comment> Comments { get; } = [];

        public Task<Comment> CreateAsync(Comment comment)
        {
            comment.Id = Comments.Count + 1;
            Comments.Add(comment);
            return Task.FromResult(comment);
        }

        public Task<List<Comment>> GetByTicketIdAsync(int ticketId)
        {
            return Task.FromResult(Comments.Where(comment => comment.TicketId == ticketId).ToList());
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

    private sealed class FakeNotificationService : INotificationService
    {
        public List<NotificationResponseDto> Notifications { get; } = [];

        public Task NotifyAsync(int userId, int? ticketId, string title, string message)
        {
            Notifications.Add(new NotificationResponseDto
            {
                Id = Notifications.Count + 1,
                UserId = userId,
                TicketId = ticketId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow
            });

            return Task.CompletedTask;
        }

        public Task<List<NotificationResponseDto>> GetUserNotificationsAsync(int userId)
        {
            return Task.FromResult(Notifications.Where(notification => notification.UserId == userId).ToList());
        }

        public Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            return Task.FromResult(true);
        }
    }
}
