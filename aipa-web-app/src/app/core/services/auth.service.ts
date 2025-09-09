import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, of, timer, Subscription } from 'rxjs';
import { map, catchError, tap, finalize } from 'rxjs/operators';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { DOCUMENT } from '@angular/common';

// Import types from generated backend API
import { AuthService as BackendAuthService } from '../../../lib/api/backend/api/auth.service';
import { Configuration } from '../../../lib/api/backend/configuration';
import type {
  AuthenticationRequest,
  AuthenticationResponse,
  RegisterRequest,
  RegisterResponse,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  RefreshTokenRequest,
  UserInfo,
} from '../../../lib/api/backend';

export interface AuthState {
  isAuthenticated: boolean;
  user: UserInfo | null;
  accessToken: string | null;
  refreshToken: string | null;
  loading: boolean;
  error: string | null;
}

// Auth data interfaces
export interface LoginData {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterData {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  acceptTerms: boolean;
}

export interface RegisterData {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  acceptTerms: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly document = inject(DOCUMENT);

  // Check if we're in browser environment
  private get isBrowser(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }

  // Backend API Configuration
  private readonly backendConfig = new Configuration({
    basePath: this.getBackendUrl(),
    accessToken: () => this.authState.value.accessToken || '',
  });

  private readonly backendAuthApi = new BackendAuthService(
    inject(HttpClient),
    this.backendConfig.basePath || 'http://localhost:5030',
    this.backendConfig,
  );

  // Authentication State Management
  private readonly authState = new BehaviorSubject<AuthState>({
    isAuthenticated: false,
    user: null,
    accessToken: null,
    refreshToken: null,
    loading: false,
    error: null,
  });

  // Public observables
  public readonly authState$ = this.authState.asObservable();
  public readonly isAuthenticated$ = this.authState$.pipe(
    map((state) => state.isAuthenticated),
  );
  public readonly user$ = this.authState$.pipe(map((state) => state.user));
  public readonly loading$ = this.authState$.pipe(
    map((state) => state.loading),
  );
  public readonly error$ = this.authState$.pipe(map((state) => state.error));

  // Token refresh timer
  private refreshTimer: Subscription | null = null;

  constructor() {
    this.initializeAuth();
  }

  /**
   * Initialize authentication state from stored tokens
   */
  private initializeAuth(): void {
    const storedTokens = this.getStoredTokens();
    if (storedTokens.accessToken && storedTokens.refreshToken) {
      this.updateAuthState({
        isAuthenticated: true,
        accessToken: storedTokens.accessToken,
        refreshToken: storedTokens.refreshToken,
      });

      // Load user profile and setup refresh timer
      this.loadUserProfile().subscribe();
      this.setupTokenRefresh();
    }
  }

  /**
   * User login with email and password
   */
  login(credentials: LoginData): Observable<AuthenticationResponse> {
    this.setLoading(true);
    this.clearError();

    const authRequest: AuthenticationRequest = {
      email: credentials.email,
      password: credentials.password,
    };

    return this.backendAuthApi.apiAuthLoginPost(authRequest).pipe(
      tap((response) => {
        this.handleAuthSuccess(response, credentials.rememberMe);
      }),
      catchError((error) => {
        this.handleAuthError('Login failed. Please check your credentials.');
        throw error;
      }),
      finalize(() => this.setLoading(false)),
    );
  }

  /**
   * User registration
   */
  register(registerData: RegisterData): Observable<RegisterResponse> {
    this.setLoading(true);
    this.clearError();

    const registerRequest: RegisterRequest = {
      email: registerData.email,
      password: registerData.password,
      confirmPassword: registerData.confirmPassword,
      firstName: registerData.firstName,
      lastName: registerData.lastName,
      acceptTerms: registerData.acceptTerms,
    };

    return this.backendAuthApi.apiAuthRegisterPost(registerRequest).pipe(
      catchError((error) => {
        this.handleAuthError('Registration failed. Please try again.');
        throw error;
      }),
      finalize(() => this.setLoading(false)),
    );
  }

  /**
   * User logout
   */
  logout(): Observable<void> {
    this.setLoading(true);

    const refreshToken = this.authState.value.refreshToken;
    if (!refreshToken) {
      this.handleLogoutSuccess();
      return of(void 0);
    }

    return this.backendAuthApi.apiAuthLogoutPost({ refreshToken }).pipe(
      tap(() => this.handleLogoutSuccess()),
      catchError(() => {
        // Even if logout fails on server, clear local state
        this.handleLogoutSuccess();
        return of(void 0);
      }),
      finalize(() => this.setLoading(false)),
    );
  }

  /**
   * Refresh access token
   */
  refreshAccessToken(): Observable<AuthenticationResponse> {
    const refreshToken = this.authState.value.refreshToken;
    if (!refreshToken) {
      this.logout();
      throw new Error('No refresh token available');
    }

    const refreshRequest: RefreshTokenRequest = { refreshToken };

    return this.backendAuthApi.apiAuthRefreshPost(refreshRequest).pipe(
      tap((response) => {
        this.handleTokenRefresh(response);
      }),
      catchError((error) => {
        this.handleRefreshError();
        throw error;
      }),
    );
  }

  /**
   * Forgot password - send reset email
   */
  forgotPassword(email: string): Observable<void> {
    this.setLoading(true);
    this.clearError();

    const forgotRequest: ForgotPasswordRequest = { email };

    return this.backendAuthApi.apiAuthForgotPasswordPost(forgotRequest).pipe(
      map(() => void 0),
      catchError((error) => {
        this.handleAuthError('Failed to send reset email. Please try again.');
        throw error;
      }),
      finalize(() => this.setLoading(false)),
    );
  }

  /**
   * Reset password with token
   */
  resetPassword(
    email: string,
    token: string,
    newPassword: string,
  ): Observable<void> {
    this.setLoading(true);
    this.clearError();

    const resetRequest: ResetPasswordRequest = {
      email,
      token,
      password: newPassword,
      confirmPassword: newPassword,
    };

    return this.backendAuthApi.apiAuthResetPasswordPost(resetRequest).pipe(
      map(() => void 0),
      catchError((error) => {
        this.handleAuthError('Password reset failed. Please try again.');
        throw error;
      }),
      finalize(() => this.setLoading(false)),
    );
  }

  /**
   * Verify email address
   */
  verifyEmail(email: string, token: string): Observable<void> {
    this.setLoading(true);
    this.clearError();

    return this.backendAuthApi.apiAuthVerifyEmailPost(email, token).pipe(
      map(() => void 0),
      catchError((error) => {
        this.handleAuthError('Email verification failed.');
        throw error;
      }),
      finalize(() => this.setLoading(false)),
    );
  }

  /**
   * Load current user profile
   */
  loadUserProfile(): Observable<UserInfo> {
    return this.backendAuthApi.apiAuthMeGet().pipe(
      tap((user) => {
        this.updateAuthState({ user });
      }),
      catchError((error) => {
        if (error.status === 401) {
          this.logout();
        }
        throw error;
      }),
    );
  }

  /**
   * Check if user has specific role
   */
  hasRole(role: string): boolean {
    const user = this.authState.value.user;
    return user?.roles?.includes(role) || false;
  }

  /**
   * Get current access token
   */
  getAccessToken(): string | null {
    return this.authState.value.accessToken;
  }

  /**
   * Check if token is expired (client-side check)
   */
  isTokenExpired(): boolean {
    const token = this.authState.value.accessToken;
    if (!token) return true;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000; // Convert to milliseconds
      return Date.now() >= exp;
    } catch {
      return true;
    }
  }

  // Private helper methods

  private handleAuthSuccess(
    response: AuthenticationResponse,
    rememberMe = false,
  ): void {
    if (this.isBrowser) {
      const storage = rememberMe ? localStorage : sessionStorage;

      if (response.accessToken) {
        storage.setItem('access_token', response.accessToken);
      }
      if (response.refreshToken) {
        storage.setItem('refresh_token', response.refreshToken);
      }
    }

    this.updateAuthState({
      isAuthenticated: true,
      accessToken: response.accessToken || null,
      refreshToken: response.refreshToken || null,
      error: null,
    });

    this.setupTokenRefresh();
    this.loadUserProfile().subscribe();
  }

  private handleTokenRefresh(response: AuthenticationResponse): void {
    const storage = this.getStorageType();

    if (storage && response.accessToken) {
      storage.setItem('access_token', response.accessToken);
    }
    if (storage && response.refreshToken) {
      storage.setItem('refresh_token', response.refreshToken);
    }

    this.updateAuthState({
      accessToken: response.accessToken || null,
      refreshToken: response.refreshToken || null,
    });

    this.setupTokenRefresh();
  }

  private handleLogoutSuccess(): void {
    this.clearStoredTokens();
    this.clearRefreshTimer();

    this.updateAuthState({
      isAuthenticated: false,
      user: null,
      accessToken: null,
      refreshToken: null,
      error: null,
    });

    this.router.navigate(['/auth/login']);
  }

  private handleAuthError(message: string): void {
    this.updateAuthState({ error: message });
  }

  private handleRefreshError(): void {
    this.handleLogoutSuccess();
  }

  private setupTokenRefresh(): void {
    this.clearRefreshTimer();

    const token = this.authState.value.accessToken;
    if (!token) return;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000; // Convert to milliseconds
      const refreshTime = exp - Date.now() - 60000; // Refresh 1 minute before expiry

      if (refreshTime > 0) {
        this.refreshTimer = timer(refreshTime).subscribe(() => {
          this.refreshAccessToken().subscribe();
        });
      }
    } catch {
      // Invalid token format
      this.logout();
    }
  }

  private clearRefreshTimer(): void {
    if (this.refreshTimer) {
      this.refreshTimer.unsubscribe();
      this.refreshTimer = null;
    }
  }

  private getStoredTokens(): {
    accessToken: string | null;
    refreshToken: string | null;
  } {
    if (!this.isBrowser) {
      return { accessToken: null, refreshToken: null };
    }

    // Check localStorage first, then sessionStorage
    const accessToken =
      localStorage.getItem('access_token') ||
      sessionStorage.getItem('access_token');
    const refreshToken =
      localStorage.getItem('refresh_token') ||
      sessionStorage.getItem('refresh_token');

    return { accessToken, refreshToken };
  }

  private getStorageType(): Storage | null {
    if (!this.isBrowser) {
      return null;
    }
    // Determine which storage was used
    return localStorage.getItem('access_token') ? localStorage : sessionStorage;
  }

  private clearStoredTokens(): void {
    if (this.isBrowser) {
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      sessionStorage.removeItem('access_token');
      sessionStorage.removeItem('refresh_token');
    }
  }

  private updateAuthState(updates: Partial<AuthState>): void {
    this.authState.next({
      ...this.authState.value,
      ...updates,
    });
  }

  private setLoading(loading: boolean): void {
    this.updateAuthState({ loading });
  }

  private clearError(): void {
    this.updateAuthState({ error: null });
  }

  private getBackendUrl(): string {
    // For Angular, use environment configuration
    return (
      ((globalThis as Record<string, unknown>)?.['BACKEND_URL'] as string) ||
      'http://localhost:5030'
    );
  }
}
