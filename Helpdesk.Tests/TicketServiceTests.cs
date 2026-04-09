using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Application.Services;
using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;
using Xunit;

namespace Helpdesk.Tests;

public class TicketServiceTests
{
    [Fact]
    public async Task CreateTicketAsync_UsesAuthenticatedRequesterId()
    {
        var service = CreateService(out var ticketRepository, out _);
        var dto = new CreateTicketDto
        {
            Title = "Printer issue",
            Description = "The office printer stopped printing today.",
            Priority = TicketPriority.High
        };

        var result = await service.CreateTicketAsync(dto, requesterId: 7);

        Assert.Equal(7, result.RequesterId);
        Assert.Single(ticketRepository.Tickets);
        Assert.Equal(7, ticketRepository.Tickets[0].RequesterId);
        Assert.Equal(TicketStatus.New, ticketRepository.Tickets[0].Status);
    }

    [Fact]
    public async Task GetAllTicketsAsync_ForRequester_ReturnsOnlyOwnedTickets()
    {
        var service = CreateService(out var ticketRepository, out _);
        ticketRepository.Tickets.AddRange(
            new Ticket { Id = 1, Title = "A", Description = "Owned by requester.", RequesterId = 2 },
            new Ticket { Id = 2, Title = "B", Description = "Not owned by requester.", RequesterId = 9 },
            new Ticket { Id = 3, Title = "C", Description = "Also owned by requester.", RequesterId = 2 });

        var result = await service.GetAllTicketsAsync(userId: 2, role: nameof(UserRole.Requester));

        Assert.Equal(2, result.Count);
        Assert.All(result, ticket => Assert.Equal(2, ticket.RequesterId));
    }

    [Fact]
    public async Task GetTicketByIdAsync_ForAgent_ReturnsNullWhenTicketIsAssignedToAnotherAgent()
    {
        var service = CreateService(out var ticketRepository, out _);
        ticketRepository.Tickets.Add(new Ticket
        {
            Id = 4,
            Title = "VPN issue",
            Description = "Assigned to a different agent.",
            RequesterId = 2,
            AgentId = 99
        });

        var result = await service.GetTicketByIdAsync(id: 4, userId: 3, role: nameof(UserRole.Agent));

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTicketStatusAsync_RejectsInvalidTransition()
    {
        var service = CreateService(out var ticketRepository, out _);
        ticketRepository.Tickets.Add(new Ticket
        {
            Id = 5,
            Title = "Email issue",
            Description = "Assigned ticket ready for workflow test.",
            RequesterId = 2,
            AgentId = 3,
            Status = TicketStatus.Assigned
        });

        var result = await service.UpdateTicketStatusAsync(
            id: 5,
            new UpdateTicketStatusDto { Status = TicketStatus.Resolved },
            userId: 3,
            role: nameof(UserRole.Agent));

        Assert.Equal(UpdateTicketStatusResult.InvalidTransition, result);
        Assert.Equal(TicketStatus.Assigned, ticketRepository.Tickets[0].Status);
    }

    [Fact]
    public async Task UpdateTicketStatusAsync_AllowsValidTransitionForAssignedAgent()
    {
        var service = CreateService(out var ticketRepository, out _);
        ticketRepository.Tickets.Add(new Ticket
        {
            Id = 6,
            Title = "Network issue",
            Description = "Assigned ticket ready for valid workflow test.",
            RequesterId = 2,
            AgentId = 3,
            Status = TicketStatus.Assigned
        });

        var result = await service.UpdateTicketStatusAsync(
            id: 6,
            new UpdateTicketStatusDto { Status = TicketStatus.InProgress },
            userId: 3,
            role: nameof(UserRole.Agent));

        Assert.Equal(UpdateTicketStatusResult.Success, result);
        Assert.Equal(TicketStatus.InProgress, ticketRepository.Tickets[0].Status);
    }

    [Fact]
    public async Task UpdateTicketStatusAsync_RejectsAgentWhoDoesNotOwnTicket()
    {
        var service = CreateService(out var ticketRepository, out _);
        ticketRepository.Tickets.Add(new Ticket
        {
            Id = 7,
            Title = "Laptop issue",
            Description = "Assigned to another agent for authorization test.",
            RequesterId = 2,
            AgentId = 8,
            Status = TicketStatus.Assigned
        });

        var result = await service.UpdateTicketStatusAsync(
            id: 7,
            new UpdateTicketStatusDto { Status = TicketStatus.InProgress },
            userId: 3,
            role: nameof(UserRole.Agent));

        Assert.Equal(UpdateTicketStatusResult.Forbidden, result);
        Assert.Equal(TicketStatus.Assigned, ticketRepository.Tickets[0].Status);
    }

    private static TicketService CreateService(out FakeTicketRepository ticketRepository, out FakeUserRepository userRepository)
    {
        ticketRepository = new FakeTicketRepository();
        userRepository = new FakeUserRepository();
        return new TicketService(ticketRepository, userRepository);
    }

    private sealed class FakeTicketRepository : ITicketRepository
    {
        public List<Ticket> Tickets { get; } = [];

        public Task<Ticket> CreateAsync(Ticket ticket)
        {
            ticket.Id = Tickets.Count == 0 ? 1 : Tickets.Max(existing => existing.Id) + 1;
            Tickets.Add(ticket);
            return Task.FromResult(ticket);
        }

        public Task<List<Ticket>> GetAllAsync()
        {
            return Task.FromResult(Tickets.ToList());
        }

        public Task<Ticket?> GetByIdAsync(int id)
        {
            return Task.FromResult(Tickets.FirstOrDefault(ticket => ticket.Id == id));
        }

        public Task UpdateAsync(Ticket ticket)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public Task<User?> GetByEmailAsync(string email)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByIdAsync(int id)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User> CreateAsync(User user)
        {
            return Task.FromResult(user);
        }
    }
}
