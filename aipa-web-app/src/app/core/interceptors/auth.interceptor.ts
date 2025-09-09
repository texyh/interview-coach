import {
  HttpInterceptorFn,
  HttpErrorResponse,
  HttpRequest,
  HttpHandlerFn,
  HttpEvent,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, switchMap, filter, take } from 'rxjs/operators';

import { AuthService } from '../services/auth.service';

let isRefreshing = false;
const refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<
  string | null
>(null);

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> => {
  const authService = inject(AuthService);

  // Add auth token to requests
  const token = authService.getAccessToken();
  let authReq = req;

  if (token && !isAuthRequest(req.url)) {
    authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`),
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 unauthorized errors
      if (error.status === 401 && authService.getAccessToken()) {
        return handle401Error(authReq, next, authService);
      }

      return throwError(() => error);
    }),
  );
};

function handle401Error(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
): Observable<HttpEvent<unknown>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return authService.refreshAccessToken().pipe(
      switchMap((response) => {
        isRefreshing = false;
        const newToken = response.accessToken;

        if (newToken) {
          refreshTokenSubject.next(newToken);
          const newRequest = request.clone({
            headers: request.headers.set('Authorization', `Bearer ${newToken}`),
          });
          return next(newRequest);
        }

        // If refresh failed, logout user
        authService.logout();
        return throwError(() => new Error('Token refresh failed'));
      }),
      catchError((error) => {
        isRefreshing = false;
        authService.logout();
        return throwError(() => error);
      }),
    );
  }

  // If already refreshing, wait for the new token
  return refreshTokenSubject.pipe(
    filter((token) => token !== null),
    take(1),
    switchMap((token) => {
      const newRequest = request.clone({
        headers: request.headers.set('Authorization', `Bearer ${token}`),
      });
      return next(newRequest);
    }),
  );
}

function isAuthRequest(url: string): boolean {
  // Skip adding token for auth-related requests to avoid infinite loops
  const authEndpoints = [
    '/api/Auth/login',
    '/api/Auth/register',
    '/api/Auth/refresh',
    '/api/Auth/forgot-password',
  ];
  return authEndpoints.some((endpoint) => url.includes(endpoint));
}
