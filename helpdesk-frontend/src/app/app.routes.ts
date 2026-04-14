import { Routes } from '@angular/router';
import { DashboardPageComponent } from './features/dashboard/dashboard-page.component';
import { LoginPageComponent } from './features/login/login-page.component';
import { NotificationsPageComponent } from './features/notifications/notifications-page.component';
import { authGuard } from './core/auth.guard';
import { CreateTicketPageComponent } from './features/tickets/create-ticket-page.component';
import { TicketDetailPageComponent } from './features/tickets/ticket-detail-page.component';
import { TicketListPageComponent } from './features/tickets/ticket-list-page.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: LoginPageComponent },
  { path: 'dashboard', component: DashboardPageComponent, canActivate: [authGuard] },
  { path: 'notifications', component: NotificationsPageComponent, canActivate: [authGuard] },
  { path: 'tickets', component: TicketListPageComponent, canActivate: [authGuard] },
  { path: 'tickets/new', component: CreateTicketPageComponent, canActivate: [authGuard] },
  { path: 'tickets/:id', component: TicketDetailPageComponent, canActivate: [authGuard] },
];
