import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, map, of, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { tokenStorage } from '../../../core/auth/token-storage';
import { AuthResponse, UserDto } from '../models/auth-response';
import { ForgotPasswordRequest } from '../models/forgot-password';
import { Login } from '../models/login';
import { UserRegistration } from '../models/register';
import { ResetPasswordRequest } from '../models/reset-password';

const WITH_CREDENTIALS = { withCredentials: true };

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  readonly currentUser = signal<UserDto | null>(null);
  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  login(credentials: Login): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, credentials, WITH_CREDENTIALS)
      .pipe(tap((response) => this.applySession(response)));
  }

  register(payload: UserRegistration): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/register`, payload, WITH_CREDENTIALS)
      .pipe(tap((response) => this.applySession(response)));
  }

  loginWithGoogle(idToken: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/google`, { idToken }, WITH_CREDENTIALS)
      .pipe(tap((response) => this.applySession(response)));
  }

  forgotPassword(payload: ForgotPasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/forgot-password`, payload);
  }

  resetPassword(payload: ResetPasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/reset-password`, payload);
  }

  /**
   * Exchanges the HttpOnly refresh-token cookie for a new access token.
   * Used both on app startup and by the interceptor when an access token has expired mid-session.
   */
  refreshToken(): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/refresh`, {}, WITH_CREDENTIALS)
      .pipe(tap((response) => this.applySession(response)));
  }

  /** Rehydrates session state on app startup from the refresh-token cookie, if present. */
  restoreSession(): Observable<UserDto | null> {
    return this.refreshToken().pipe(
      map((response) => response.user),
      catchError(() => {
        this.clearSession();
        return of(null);
      }),
    );
  }

  logout(): void {
    this.http.post(`${this.baseUrl}/logout`, {}, WITH_CREDENTIALS).subscribe({
      next: () => this.finishLogout(),
      error: () => this.finishLogout(),
    });
  }

  private finishLogout(): void {
    this.clearSession();
    this.router.navigate(['/login']);
  }

  private applySession(response: AuthResponse): void {
    tokenStorage.set(response.token);
    this.currentUser.set(response.user);
  }

  private clearSession(): void {
    tokenStorage.clear();
    this.currentUser.set(null);
  }
}
