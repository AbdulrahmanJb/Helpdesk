import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { apiConfig } from './api.config';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenKey = 'helpdesk_token';
  private readonly roleKey = 'helpdesk_role';
  private readonly emailKey = 'helpdesk_email';

  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${apiConfig.baseUrl}/Auth/login`, payload)
      .pipe(tap((response) => this.storeSession(response)));
  }

  getToken(): string | null {
    return this.getStorageItem(this.tokenKey);
  }

  getRole(): string | null {
    return this.getStorageItem(this.roleKey);
  }

  getEmail(): string | null {
    return this.getStorageItem(this.emailKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  hasRole(role: string): boolean {
    return this.getRole() === role;
  }

  logout(): void {
    if (typeof localStorage === 'undefined') {
      return;
    }

    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.roleKey);
    localStorage.removeItem(this.emailKey);
  }

  private storeSession(response: LoginResponse): void {
    if (typeof localStorage === 'undefined') {
      return;
    }

    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.roleKey, response.role);
    localStorage.setItem(this.emailKey, response.email);
  }

  private getStorageItem(key: string): string | null {
    if (typeof localStorage === 'undefined') {
      return null;
    }

    return localStorage.getItem(key);
  }
}
