import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { apiConfig } from '../../core/api.config';

export interface NotificationItem {
  id: number;
  userId: number;
  ticketId: number | null;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly http = inject(HttpClient);
  readonly unreadCount = signal(0);

  getNotifications(): Observable<NotificationItem[]> {
    return this.http
      .get<NotificationItem[]>(`${apiConfig.baseUrl}/Notifications`)
      .pipe(tap((notifications) => this.syncUnreadCount(notifications)));
  }

  markAsRead(id: number): Observable<void> {
    return this.http.put<void>(`${apiConfig.baseUrl}/Notifications/${id}/read`, {});
  }

  loadUnreadCount(): Observable<NotificationItem[]> {
    return this.getNotifications();
  }

  setUnreadCountFromNotifications(notifications: NotificationItem[]): void {
    this.syncUnreadCount(notifications);
  }

  decrementUnreadCount(): void {
    this.unreadCount.update((count) => Math.max(0, count - 1));
  }

  private syncUnreadCount(notifications: NotificationItem[]): void {
    this.unreadCount.set(notifications.filter((notification) => !notification.isRead).length);
  }
}
