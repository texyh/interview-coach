import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzAvatarModule } from 'ng-zorro-antd/avatar';
import { ButtonComponent } from '../button/button.component';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    NzIconModule,
    NzAvatarModule,
    ButtonComponent,
    ThemeToggleComponent,
  ],
  template: `
    <nav class="navbar">
      <div class="navbar-container">
        <!-- Logo/Brand -->
        <div class="navbar-brand">
          <a routerLink="/" class="brand-link">
            <div class="brand-icon">
              <i nz-icon nzType="robot" nzTheme="outline" class="text-2xl"></i>
            </div>
            <span class="brand-text">AIPA</span>
          </a>
        </div>

        <!-- Main Navigation -->
        <div class="navbar-nav">
          <a
            routerLink="/home"
            routerLinkActive="nav-link-active"
            class="nav-link"
          >
            Home
          </a>
          <a
            routerLink="/practice"
            routerLinkActive="nav-link-active"
            class="nav-link"
          >
            Practice
          </a>
          <a
            routerLink="/mock-interviews"
            routerLinkActive="nav-link-active"
            class="nav-link"
          >
            Mock Interviews
          </a>
          <a
            routerLink="/analytics"
            routerLinkActive="nav-link-active"
            class="nav-link"
          >
            Analytics
          </a>
          <a
            routerLink="/resources"
            routerLinkActive="nav-link-active"
            class="nav-link"
          >
            Resources
          </a>
        </div>

        <!-- Right Side Actions -->
        <div class="navbar-actions">
          <app-theme-toggle></app-theme-toggle>

          <!-- Authenticated User Menu -->
          <div
            class="auth-section"
            *ngIf="isAuthenticated$ | async; else guestActions"
          >
            <div class="user-info">
              <nz-avatar nzSize="small" nzIcon="user"></nz-avatar>
              <span class="user-name">{{
                (user$ | async)?.fullName || (user$ | async)?.email
              }}</span>
            </div>
            <app-button
              variant="ghost"
              size="sm"
              (buttonClick)="handleSignOut()"
            >
              Sign Out
            </app-button>
          </div>

          <!-- Guest Actions -->
          <ng-template #guestActions>
            <div class="auth-buttons">
              <app-button
                variant="ghost"
                size="sm"
                (buttonClick)="handleSignIn()"
              >
                Sign In
              </app-button>
              <app-button
                variant="primary"
                size="sm"
                (buttonClick)="handleSignUp()"
              >
                Get Started
              </app-button>
            </div>
          </ng-template>
        </div>

        <!-- Mobile Menu Button -->
        <div class="mobile-menu-button lg:hidden">
          <button
            class="mobile-toggle"
            (click)="toggleMobileMenu()"
            [attr.aria-expanded]="isMobileMenuOpen"
          >
            <i
              nz-icon
              [nzType]="isMobileMenuOpen ? 'close' : 'menu'"
              nzTheme="outline"
              class="text-xl"
            ></i>
          </button>
        </div>
      </div>

      <!-- Mobile Menu -->
      <div
        class="mobile-menu"
        [class.mobile-menu-open]="isMobileMenuOpen"
        [@slideDown]="isMobileMenuOpen ? 'open' : 'closed'"
      >
        <div class="mobile-menu-content">
          <a
            routerLink="/home"
            routerLinkActive="mobile-nav-link-active"
            class="mobile-nav-link"
            (click)="closeMobileMenu()"
          >
            Home
          </a>
          <a
            routerLink="/practice"
            routerLinkActive="mobile-nav-link-active"
            class="mobile-nav-link"
            (click)="closeMobileMenu()"
          >
            Practice
          </a>
          <a
            routerLink="/mock-interviews"
            routerLinkActive="mobile-nav-link-active"
            class="mobile-nav-link"
            (click)="closeMobileMenu()"
          >
            Mock Interviews
          </a>
          <a
            routerLink="/analytics"
            routerLinkActive="mobile-nav-link-active"
            class="mobile-nav-link"
            (click)="closeMobileMenu()"
          >
            Analytics
          </a>
          <a
            routerLink="/resources"
            routerLinkActive="mobile-nav-link-active"
            class="mobile-nav-link"
            (click)="closeMobileMenu()"
          >
            Resources
          </a>

          <div
            class="mobile-auth-section"
            *ngIf="isAuthenticated$ | async; else mobileGuestActions"
          >
            <a
              routerLink="/dashboard"
              routerLinkActive="mobile-nav-link-active"
              class="mobile-nav-link"
              (click)="closeMobileMenu()"
            >
              <i nz-icon nzType="dashboard"></i>
              Dashboard
            </a>
            <a class="mobile-nav-link" (click)="handleSignOut()">
              <i nz-icon nzType="logout"></i>
              Sign Out
            </a>
          </div>

          <ng-template #mobileGuestActions>
            <div class="mobile-auth-buttons">
              <app-button
                variant="ghost"
                size="sm"
                [fullWidth]="true"
                (buttonClick)="handleSignIn()"
              >
                Sign In
              </app-button>
              <app-button
                variant="primary"
                size="sm"
                [fullWidth]="true"
                (buttonClick)="handleSignUp()"
              >
                Get Started
              </app-button>
            </div>
          </ng-template>
        </div>
      </div>
    </nav>
  `,
  styles: [
    `
      .navbar {
        @apply bg-white dark:bg-slate-900 border-b border-slate-200 dark:border-slate-800;
        @apply sticky top-0 z-50 backdrop-blur-sm bg-white/95 dark:bg-slate-900/95;
        @apply transition-all duration-200;
      }

      .navbar-container {
        @apply max-w-7xl mx-auto px-4 sm:px-6 lg:px-8;
        @apply flex items-center justify-between h-16;
      }

      /* Brand/Logo */
      .navbar-brand {
        @apply flex-shrink-0;
      }

      .brand-link {
        @apply flex items-center space-x-3 text-slate-900 dark:text-slate-100;
        @apply hover:text-indigo-600 dark:hover:text-indigo-400;
        @apply transition-colors duration-200 no-underline;
      }

      .brand-icon {
        @apply w-8 h-8 bg-indigo-600 text-white rounded-lg;
        @apply flex items-center justify-center;
      }

      .brand-text {
        @apply text-xl font-bold;
      }

      /* Main Navigation */
      .navbar-nav {
        @apply hidden lg:flex items-center space-x-8;
      }

      .nav-link {
        @apply text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-100;
        @apply font-medium transition-colors duration-200 no-underline;
        @apply px-3 py-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800;
      }

      .nav-link-active {
        @apply text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-900/20;
      }

      /* Right Side Actions */
      .navbar-actions {
        @apply hidden lg:flex items-center space-x-4;
      }

      .auth-buttons {
        @apply flex items-center space-x-3;
      }

      .auth-section {
        @apply flex items-center space-x-4;
      }

      .user-info {
        @apply flex items-center space-x-2;
      }

      .user-name {
        @apply text-slate-700 dark:text-slate-300 font-medium;
      }

      /* Mobile Menu */
      .mobile-menu-button {
        @apply flex items-center;
      }

      .mobile-toggle {
        @apply p-2 rounded-lg text-slate-600 hover:text-slate-900;
        @apply hover:bg-slate-100 dark:text-slate-400 dark:hover:text-slate-100;
        @apply dark:hover:bg-slate-800 transition-colors duration-200;
        @apply focus:outline-none focus:ring-2 focus:ring-indigo-500;
      }

      .mobile-menu {
        @apply lg:hidden border-t border-slate-200 dark:border-slate-800;
        @apply bg-white dark:bg-slate-900;
        @apply max-h-0 overflow-hidden transition-all duration-300 ease-in-out;
      }

      .mobile-menu-open {
        @apply max-h-96;
      }

      .mobile-menu-content {
        @apply px-4 py-6 space-y-3;
      }

      .mobile-nav-link {
        @apply block px-4 py-3 text-slate-600 hover:text-slate-900;
        @apply dark:text-slate-400 dark:hover:text-slate-100;
        @apply hover:bg-slate-100 dark:hover:bg-slate-800;
        @apply rounded-lg transition-colors duration-200 no-underline;
        @apply font-medium;
      }

      .mobile-nav-link-active {
        @apply text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-900/20;
      }

      .mobile-auth-section {
        @apply border-t border-slate-200 dark:border-slate-700 pt-4 mt-4;
        @apply space-y-2;
      }

      .mobile-auth-buttons {
        @apply border-t border-slate-200 dark:border-slate-700 pt-4 mt-4;
        @apply space-y-3;
      } /* Responsive adjustments */
      @media (max-width: 1024px) {
        .navbar-container {
          @apply px-4;
        }
      }

      /* Hover effects */
      .navbar:hover {
        @apply bg-white/95 dark:bg-slate-900/95;
      }

      /* Focus states for accessibility */
      .nav-link:focus,
      .mobile-nav-link:focus {
        @apply outline-none ring-2 ring-indigo-500 ring-offset-2;
        @apply ring-offset-white dark:ring-offset-slate-900;
      }
    `,
  ],
  animations: [
    // You can add Angular animations here if needed
  ],
})
export class NavbarComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  isMobileMenuOpen = false;

  // Auth observables
  protected readonly isAuthenticated$ = this.authService.isAuthenticated$;
  protected readonly user$ = this.authService.user$;

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }

  handleSignIn(): void {
    this.router.navigate(['/auth/login']);
    this.closeMobileMenu();
  }

  handleSignUp(): void {
    this.router.navigate(['/auth/register']);
    this.closeMobileMenu();
  }

  handleSignOut(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/home']);
        this.closeMobileMenu();
      },
    });
  }
}
