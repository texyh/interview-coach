import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzAvatarModule } from 'ng-zorro-antd/avatar';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    NzLayoutModule,
    NzMenuModule,
    NzIconModule,
    NzButtonModule,
    NzDropDownModule,
    NzAvatarModule,
  ],
  template: `
    <nz-layout class="layout">
      <nz-header class="header">
        <div class="logo">
          <i nz-icon nzType="robot" nzTheme="outline"></i>
          <span>AIPA</span>
        </div>

        <div class="user-menu">
          <nz-dropdown [nzTrigger]="'click'">
            <a nz-dropdown class="user-dropdown">
              <nz-avatar nzSize="small" nzIcon="user"></nz-avatar>
              <span class="user-name">{{
                (currentUser | async)?.fullName || (currentUser | async)?.email
              }}</span>
              <i nz-icon nzType="down"></i>
            </a>
            <ul nz-menu nzInDropdown>
              <li nz-menu-item>
                <i nz-icon nzType="user"></i>
                Profile
              </li>
              <li nz-menu-item>
                <i nz-icon nzType="setting"></i>
                Settings
              </li>
              <li nz-menu-divider></li>
              <li nz-menu-item (click)="logout()">
                <i nz-icon nzType="logout"></i>
                Sign Out
              </li>
            </ul>
          </nz-dropdown>
        </div>
      </nz-header>

      <nz-content class="content">
        <div class="inner-content">
          <h1>Welcome to AIPA Dashboard</h1>
          <p>
            Your AI-powered interview preparation assistant is ready to help you
            succeed!
          </p>
        </div>
      </nz-content>
    </nz-layout>
  `,
  styles: [
    `
      .layout {
        @apply min-h-screen;
      }

      .header {
        @apply flex items-center justify-between px-6;
        @apply bg-white dark:bg-slate-800 border-b border-slate-200 dark:border-slate-700;
      }

      .logo {
        @apply flex items-center space-x-2 text-xl font-bold;
        @apply text-indigo-600 dark:text-indigo-400;
      }

      .logo i {
        @apply text-2xl;
      }

      .user-menu {
        @apply flex items-center;
      }

      .user-dropdown {
        @apply flex items-center space-x-2 px-3 py-2 rounded-md;
        @apply hover:bg-slate-100 dark:hover:bg-slate-700;
        @apply transition-colors duration-200;
      }

      .user-name {
        @apply text-slate-700 dark:text-slate-300;
      }

      .content {
        @apply p-6 bg-slate-50 dark:bg-slate-900;
      }

      .inner-content {
        @apply max-w-4xl mx-auto;
      }

      .inner-content h1 {
        @apply text-3xl font-bold text-slate-900 dark:text-slate-100 mb-4;
      }

      .inner-content p {
        @apply text-lg text-slate-600 dark:text-slate-400;
      }
    `,
  ],
})
export class DashboardComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly currentUser = this.authService.user$;

  protected logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/auth/login']);
      },
    });
  }
}
