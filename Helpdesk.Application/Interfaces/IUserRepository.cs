using Helpdesk.Domain.Entities;

namespace Helpdesk.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User> CreateAsync(User user);
}