import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { NotificationItem, NotificationService } from '../notifications/notification.service';
import { Ticket, TicketService } from '../tickets/ticket.service';

@Component({
  selector: 'app-dashboard-page',
  imports: [CommonModule, RouterLink, DatePipe],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.css',
})
export class DashboardPageComponent {
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly ticketService = inject(TicketService);

  protected readonly email = this.authService.getEmail();
  protected readonly role = this.authService.getRole();
  protected readonly unreadCount = this.notificationService.unreadCount;
  protected readonly tickets = signal<Ticket[]>([]);
  protected readonly notifications = signal<NotificationItem[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal('');

  constructor() {
    this.loadDashboardData();
  }

  protected reload(): void {
    this.loadDashboardData();
  }

  protected getOpenTicketCount(): number {
    return this.tickets().filter((ticket) => ticket.status < 4).length;
  }

  protected getResolvedTicketCount(): number {
    return this.tickets().filter((ticket) => ticket.status === 3 || ticket.status === 4).length;
  }

  protected getAssignedTicketCount(): number {
    return this.tickets().filter((ticket) => ticket.agentId !== null).length;
  }

  protected getRecentTickets(): Ticket[] {
    return [...this.tickets()]
      .sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime())
      .slice(0, 4);
  }

  protected getRecentNotifications(): NotificationItem[] {
    return [...this.notifications()]
      .sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime())
      .slice(0, 4);
  }

  protected getStatusLabel(status: number): string {
    return ['New', 'Assigned', 'In Progress', 'Resolved', 'Closed'][status] ?? 'Unknown';
  }

  protected getFocusLabel(): string {
    switch (this.role) {
      case 'Admin':
        return 'Admin overview';
      case 'Agent':
        return 'Assigned work view';
      default:
        return 'Requester view';
    }
  }

  protected getFocusDescription(): string {
    switch (this.role) {
      case 'Admin':
        return 'Track overall workload, assign tickets, and keep the helpdesk queue healthy.';
      case 'Agent':
        return 'Focus on active tickets, recent updates, and anything waiting for your next action.';
      default:
        return 'Watch your requests, recent updates, and the latest responses from the support team.';
    }
  }

  private loadDashboardData(): void {
    this.errorMessage.set('');
    this.isLoading.set(true);

    forkJoin({
      tickets: this.ticketService.getTickets(),
      notifications: this.notificationService.getNotifications(),
    })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: ({ tickets, notifications }) => {
          this.tickets.set(tickets);
          this.notifications.set(notifications);
          this.notificationService.setUnreadCountFromNotifications(notifications);
        },
        error: (error) => {
          const message =
            error?.error?.message ??
            error?.message ??
            'We could not load the dashboard right now.';
          this.errorMessage.set(message);
        },
      });
  }
}
