using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Application.Services;
using Helpdesk.Domain.Entities;
using Xunit;

namespace Helpdesk.Tests;

public class NotificationServiceTests
{
    [Fact]
    public async Task GetUserNotificationsAsync_ReturnsNotificationsInNewestFirstOrder()
    {
        var repository = new FakeNotificationRepository();
        repository.Notifications.AddRange(
            new Notification { Id = 1, UserId = 2, Title = "Older", Message = "Old", CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
            new Notification { Id = 2, UserId = 2, Title = "Newer", Message = "New", CreatedAt = DateTime.UtcNow });

        var service = new NotificationService(repository);

        var result = await service.GetUserNotificationsAsync(2);

        Assert.Equal(2, result.Count);
        Assert.Equal("Newer", result[0].Title);
    }

    [Fact]
    public async Task MarkAsReadAsync_OnlyMarksNotificationsOwnedByUser()
    {
        var repository = new FakeNotificationRepository();
        repository.Notifications.Add(new Notification
        {
            Id = 4,
            UserId = 3,
            Title = "Assigned",
            Message = "Ticket assigned",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        var service = new NotificationService(repository);

        var wrongUserResult = await service.MarkAsReadAsync(4, 2);
        var ownerResult = await service.MarkAsReadAsync(4, 3);

        Assert.False(wrongUserResult);
        Assert.True(ownerResult);
        Assert.True(repository.Notifications[0].IsRead);
    }

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        public List<Notification> Notifications { get; } = [];

        public Task<Notification> CreateAsync(Notification notification)
        {
            Notifications.Add(notification);
            return Task.FromResult(notification);
        }

        public Task<List<Notification>> GetByUserIdAsync(int userId)
        {
            return Task.FromResult(Notifications.Where(notification => notification.UserId == userId).ToList());
        }

        public Task<Notification?> GetByIdAsync(int id)
        {
            return Task.FromResult(Notifications.FirstOrDefault(notification => notification.Id == id));
        }

        public Task UpdateAsync(Notification notification)
        {
            return Task.CompletedTask;
        }
    }
}
