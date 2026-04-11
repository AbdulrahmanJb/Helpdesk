using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;

namespace Helpdesk.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task NotifyAsync(int userId, int? ticketId, string title, string message)
    {
        await _notificationRepository.CreateAsync(new Notification
        {
            UserId = userId,
            TicketId = ticketId,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<List<NotificationResponseDto>> GetUserNotificationsAsync(int userId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);

        return notifications
            .OrderByDescending(notification => notification.CreatedAt)
            .Select(notification => new NotificationResponseDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                TicketId = notification.TicketId,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            })
            .ToList();
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);

        if (notification == null || notification.UserId != userId)
            return false;

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
        }

        return true;
    }
}
