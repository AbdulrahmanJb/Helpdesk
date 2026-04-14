import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from './core/auth.service';
import { NotificationService } from './features/notifications/notification.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly unreadCount = this.notificationService.unreadCount;
  protected readonly currentUrl = signal(this.router.url);

  constructor() {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event) => {
        this.currentUrl.set((event as NavigationEnd).urlAfterRedirects);

        if (this.authService.isAuthenticated()) {
          this.notificationService.loadUnreadCount().subscribe({
            error: () => undefined,
          });
        }
      });

    if (this.authService.isAuthenticated()) {
      this.notificationService.loadUnreadCount().subscribe({
        error: () => undefined,
      });
    }
  }

  protected showSidebar(): boolean {
    return this.authService.isAuthenticated() && this.currentUrl() !== '/login';
  }

  protected getDisplayName(): string {
    const email = this.authService.getEmail() ?? '';

    if (!email) {
      return 'Workspace User';
    }

    return email
      .replace('@helpdesk.com', '')
      .replace('@test.com', '')
      .replace(/[._-]/g, ' ')
      .replace(/\b\w/g, (character) => character.toUpperCase());
  }

  protected getCurrentRole(): string {
    return this.authService.getRole() ?? '';
  }

  protected logout(): void {
    this.authService.logout();
    void this.router.navigateByUrl('/login');
  }
}
