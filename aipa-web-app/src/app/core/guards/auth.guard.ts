import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, take } from 'rxjs/operators';

import { AuthService } from '../services/auth.service';

/**
 * Route guard that checks if user is authenticated
 * Redirects to login page if not authenticated
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated$.pipe(
    take(1),
    map((isAuthenticated) => {
      if (isAuthenticated) {
        return true;
      } else {
        router.navigate(['/auth/login']);
        return false;
      }
    }),
  );
};

/**
 * Route guard that checks if user is NOT authenticated
 * Redirects to dashboard if already authenticated (for login/register pages)
 */
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated$.pipe(
    take(1),
    map((isAuthenticated) => {
      if (!isAuthenticated) {
        return true;
      } else {
        router.navigate(['/dashboard']);
        return false;
      }
    }),
  );
};

/**
 * Role-based route guard
 * Checks if user has required role
 */
export const roleGuard = (requiredRole: string): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    return authService.user$.pipe(
      take(1),
      map((user) => {
        if (user && authService.hasRole(requiredRole)) {
          return true;
        } else {
          router.navigate(['/unauthorized']);
          return false;
        }
      }),
    );
  };
};

/**
 * Admin role guard (convenience function)
 */
export const adminGuard: CanActivateFn = roleGuard('Admin');

/**
 * Premium user guard (convenience function)
 */
export const premiumGuard: CanActivateFn = roleGuard('Premium');
