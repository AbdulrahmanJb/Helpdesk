import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { NotificationService } from '../notifications/notification.service';
import { Ticket, TicketService } from './ticket.service';

@Component({
  selector: 'app-ticket-list-page',
  imports: [CommonModule, RouterLink, DatePipe],
  templateUrl: './ticket-list-page.component.html',
  styleUrl: './ticket-list-page.component.css',
})
export class TicketListPageComponent {
  private readonly ticketService = inject(TicketService);
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly tickets = signal<Ticket[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal('');
  protected readonly role = this.authService.getRole();
  protected readonly email = this.authService.getEmail();
  protected readonly unreadCount = this.notificationService.unreadCount;

  constructor() {
    this.notificationService.loadUnreadCount().subscribe({
      error: () => undefined,
    });
    this.loadTickets();
  }

  protected logout(): void {
    this.authService.logout();
    void this.router.navigateByUrl('/login');
  }

  protected reload(): void {
    this.loadTickets();
  }

  protected getStatusLabel(status: number): string {
    return ['New', 'Assigned', 'In Progress', 'Resolved', 'Closed'][status] ?? 'Unknown';
  }

  protected getPriorityLabel(priority: number): string {
    return ['Low', 'Medium', 'High'][priority] ?? 'Unknown';
  }

  private loadTickets(): void {
    this.errorMessage.set('');
    this.isLoading.set(true);

    this.ticketService
      .getTickets()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (tickets) => this.tickets.set(tickets),
        error: (error) => {
          const message =
            error?.error?.message ??
            error?.message ??
            'We could not load tickets right now.';
          this.errorMessage.set(message);
        },
      });
  }
}
