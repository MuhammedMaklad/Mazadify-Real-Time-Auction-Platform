import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest, UserInfo } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5000/api/auth';

  private readonly _accessToken = signal<string | null>(null);
  private readonly _user = signal<UserInfo | null>(null);

  readonly isAuthenticated = computed(() => this._accessToken() !== null);
  readonly currentUser = computed(() => this._user());
  readonly accessToken = computed(() => this._accessToken());

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => this.storeSession(response))
    );
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request, {
      withCredentials: true
    }).pipe(
      tap(response => this.storeSession(response))
    );
  }

  refresh(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, {}, {
      withCredentials: true
    }).pipe(
      tap(response => this.storeSession(response)),
      catchError(err => {
        this.clearSession();
        return throwError(() => err);
      })
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/logout`, {}, {
      withCredentials: true
    }).pipe(
      tap(() => this.clearSession()),
      catchError(() => {
        this.clearSession();
        return throwError(() => new Error('Logout failed'));
      })
    );
  }

  hasRole(role: string): boolean {
    return this._user()?.roles.includes(role) ?? false;
  }

  private storeSession(response: AuthResponse): void {
    this._accessToken.set(response.accessToken);
    this._user.set(response.user);
  }

  private clearSession(): void {
    this._accessToken.set(null);
    this._user.set(null);
  }
}
