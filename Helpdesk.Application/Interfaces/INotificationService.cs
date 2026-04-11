using Helpdesk.Application.DTOs;

namespace Helpdesk.Application.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(int userId, int? ticketId, string title, string message);

    Task<List<NotificationResponseDto>> GetUserNotificationsAsync(int userId);

    Task<bool> MarkAsReadAsync(int notificationId, int userId);
}
