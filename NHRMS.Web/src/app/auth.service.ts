import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';

const AUTH_URL = 'http://localhost:5209/api/auth';
const TOKEN_KEY = 'nhrms.accessToken';
const EXPIRY_KEY = 'nhrms.expiresAtUtc';
const PERMISSIONS_KEY = 'nhrms.permissions';

export interface AccessTokenResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  userName: string;
  roles: string[];
  permissions: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  login(userName: string, password: string): Observable<AccessTokenResponse> {
    return this.http.post<AccessTokenResponse>(`${AUTH_URL}/login`, { userName, password }).pipe(tap(response => this.store(response)));
  }

  bootstrap(
    key: string,
    request: { companyCode: string; firstName: string; lastName: string; email: string; userName: string; password: string; dateOfJoining: string },
  ): Observable<AccessTokenResponse> {
    const headers = new HttpHeaders({ 'X-Bootstrap-Key': key });
    return this.http.post<AccessTokenResponse>(`${AUTH_URL}/bootstrap`, request, { headers }).pipe(tap(response => this.store(response)));
  }

  get accessToken(): string | null {
    if (new Date(sessionStorage.getItem(EXPIRY_KEY) ?? 0).getTime() <= Date.now()) {
      this.logout();
      return null;
    }
    return sessionStorage.getItem(TOKEN_KEY);
  }

  get isAuthenticated(): boolean {
    return this.accessToken !== null;
  }

  hasPermission(permission: string): boolean {
    if (!this.isAuthenticated) return false;
    try {
      return (JSON.parse(sessionStorage.getItem(PERMISSIONS_KEY) ?? '[]') as string[])
        .some(value => value.toUpperCase() === permission.toUpperCase());
    } catch {
      return false;
    }
  }

  logout(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(EXPIRY_KEY);
    sessionStorage.removeItem(PERMISSIONS_KEY);
  }

  private store(response: AccessTokenResponse): void {
    sessionStorage.setItem(TOKEN_KEY, response.accessToken);
    sessionStorage.setItem(EXPIRY_KEY, response.expiresAtUtc);
    sessionStorage.setItem(PERMISSIONS_KEY, JSON.stringify(response.permissions));
  }
}
