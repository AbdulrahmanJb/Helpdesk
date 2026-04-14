import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { NotificationItem, NotificationService } from './notification.service';

@Component({
  selector: 'app-notifications-page',
  imports: [CommonModule, RouterLink, DatePipe],
  templateUrl: './notifications-page.component.html',
  styleUrl: './notifications-page.component.css',
})
export class NotificationsPageComponent {
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);

  protected readonly notifications = signal<NotificationItem[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal('');
  protected readonly busyNotificationIds = signal<number[]>([]);
  protected readonly activeFilter = signal<'all' | 'unread' | 'read'>('all');
  protected readonly email = this.authService.getEmail();
  protected readonly role = this.authService.getRole();
  protected readonly unreadCount = this.notificationService.unreadCount;

  constructor() {
    this.route.queryParamMap.subscribe((params) => {
      const filter = params.get('filter');
      if (filter === 'unread' || filter === 'read') {
        this.activeFilter.set(filter);
      } else {
        this.activeFilter.set('all');
      }
    });
    this.loadNotifications();
  }

  protected reload(): void {
    this.loadNotifications();
  }

  protected logout(): void {
    this.authService.logout();
    void this.router.navigateByUrl('/login');
  }

  protected isBusy(notificationId: number): boolean {
    return this.busyNotificationIds().includes(notificationId);
  }

  protected setFilter(filter: 'all' | 'unread' | 'read'): void {
    this.activeFilter.set(filter);
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: filter === 'all' ? {} : { filter },
      queryParamsHandling: ''
    });
  }

  protected getFilteredNotifications(): NotificationItem[] {
    switch (this.activeFilter()) {
      case 'unread':
        return this.notifications().filter((notification) => !notification.isRead);
      case 'read':
        return this.notifications().filter((notification) => notification.isRead);
      default:
        return this.notifications();
    }
  }

  protected markAsRead(notificationId: number): void {
    if (this.isBusy(notificationId)) {
      return;
    }

    this.busyNotificationIds.update((ids) => [...ids, notificationId]);

    this.notificationService
      .markAsRead(notificationId)
      .pipe(
        finalize(() =>
          this.busyNotificationIds.update((ids) => ids.filter((id) => id !== notificationId))
        )
      )
      .subscribe({
        next: () => {
          this.notifications.update((items) =>
            items.map((item) =>
              item.id === notificationId ? { ...item, isRead: true } : item
            )
          );
          this.notificationService.decrementUnreadCount();
        },
        error: (error) => {
          const message =
            error?.error?.message ??
            error?.message ??
            'We could not update the notification right now.';
          this.errorMessage.set(message);
        },
      });
  }

  private loadNotifications(): void {
    this.errorMessage.set('');
    this.isLoading.set(true);

    this.notificationService
      .getNotifications()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (notifications) => {
          this.notifications.set(notifications);
          this.notificationService.setUnreadCountFromNotifications(notifications);
        },
        error: (error) => {
          const message =
            error?.error?.message ??
            error?.message ??
            'We could not load notifications right now.';
          this.errorMessage.set(message);
        },
      });
  }
}
