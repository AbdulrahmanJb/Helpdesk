using Helpdesk.Domain.Entities;

namespace Helpdesk.Application.Interfaces;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);

    Task<List<Notification>> GetByUserIdAsync(int userId);

    Task<Notification?> GetByIdAsync(int id);

    Task UpdateAsync(Notification notification);
}
