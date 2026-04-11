using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly HelpdeskDbContext _context;

    public NotificationRepository(HelpdeskDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<List<Notification>> GetByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .Where(notification => notification.UserId == userId)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _context.Notifications.FirstOrDefaultAsync(notification => notification.Id == id);
    }

    public async Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }
}
