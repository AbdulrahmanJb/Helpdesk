import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
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
  private readonly router = inject(Router);

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

  protected goToTickets(): void {
    void this.router.navigateByUrl('/tickets');
  }

  protected logout(): void {
    this.authService.logout();
    void this.router.navigateByUrl('/login');
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

  protected getDisplayName(): string {
    const email = this.email ?? '';

    if (!email) {
      return 'Workspace User';
    }

    return email
      .replace('@helpdesk.com', '')
      .replace('@test.com', '')
      .replace(/[._-]/g, ' ')
      .replace(/\b\w/g, (character) => character.toUpperCase());
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
