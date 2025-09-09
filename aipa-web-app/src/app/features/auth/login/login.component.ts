import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCheckboxModule } from 'ng-zorro-antd/checkbox';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { finalize } from 'rxjs/operators';

import { AuthService, LoginData } from '../../../core/services/auth.service';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    NzFormModule,
    NzInputModule,
    NzButtonModule,
    NzCheckboxModule,
    NzIconModule,
    NzAlertModule,
    NzGridModule,
    NzCardModule,
    NzDividerModule,
    ButtonComponent,
    CardComponent,
  ],
  template: `
    <div class="login-container">
      <div class="login-content">
        <app-card variant="elevated" class="login-card">
          <!-- Header -->
          <div class="login-header">
            <div class="brand-logo">
              <i
                nz-icon
                nzType="robot"
                nzTheme="outline"
                class="brand-icon"
              ></i>
              <span class="brand-text">AIPA</span>
            </div>
            <h1 class="login-title">Welcome Back</h1>
            <p class="login-subtitle">Sign in to your account to continue</p>
          </div>

          <!-- Error Alert -->
          <nz-alert
            *ngIf="error()"
            nzType="error"
            [nzMessage]="error()"
            nzCloseable
            (nzOnClose)="clearError()"
            class="login-error"
          >
          </nz-alert>

          <!-- Login Form -->
          <form
            nz-form
            [formGroup]="loginForm"
            (ngSubmit)="onSubmit()"
            class="login-form"
          >
            <!-- Email Field -->
            <nz-form-item>
              <nz-form-label nzRequired nzFor="email">Email</nz-form-label>
              <nz-form-control nzHasFeedback [nzErrorTip]="emailErrorTip">
                <input
                  nz-input
                  formControlName="email"
                  id="email"
                  type="email"
                  placeholder="Enter your email address"
                  [class.error]="isFieldInvalid('email')"
                />
                <ng-template #emailErrorTip let-control>
                  <ng-container
                    [ngSwitch]="control.errors?.['required'] && 'required'"
                  >
                    <span *ngSwitchCase="'required'">Email is required</span>
                    <span *ngSwitchDefault
                      >Please enter a valid email address</span
                    >
                  </ng-container>
                </ng-template>
              </nz-form-control>
            </nz-form-item>

            <!-- Password Field -->
            <nz-form-item>
              <nz-form-label nzRequired nzFor="password"
                >Password</nz-form-label
              >
              <nz-form-control nzHasFeedback [nzErrorTip]="passwordErrorTip">
                <nz-input-group [nzSuffix]="suffixTemplate">
                  <input
                    nz-input
                    formControlName="password"
                    id="password"
                    [type]="passwordVisible() ? 'text' : 'password'"
                    placeholder="Enter your password"
                    [class.error]="isFieldInvalid('password')"
                  />
                </nz-input-group>
                <ng-template #suffixTemplate>
                  <i
                    nz-icon
                    [nzType]="passwordVisible() ? 'eye-invisible' : 'eye'"
                    (click)="togglePasswordVisibility()"
                    class="password-toggle"
                  >
                  </i>
                </ng-template>
                <ng-template #passwordErrorTip let-control>
                  <span *ngIf="control.errors?.['required']"
                    >Password is required</span
                  >
                </ng-template>
              </nz-form-control>
            </nz-form-item>

            <!-- Remember Me & Forgot Password -->
            <nz-form-item class="login-options">
              <div class="login-options-content">
                <label nz-checkbox formControlName="rememberMe">
                  Remember me
                </label>
                <a
                  routerLink="/auth/forgot-password"
                  class="forgot-password-link"
                >
                  Forgot password?
                </a>
              </div>
            </nz-form-item>

            <!-- Submit Button -->
            <nz-form-item class="login-submit">
              <app-button
                type="submit"
                variant="primary"
                size="lg"
                [fullWidth]="true"
                [disabled]="loginForm.invalid || loading()"
              >
                {{ loading() ? 'Signing In...' : 'Sign In' }}
              </app-button>
            </nz-form-item>
          </form>

          <!-- Social Login -->
          <nz-divider nzText="or continue with"></nz-divider>

          <div class="social-login">
            <app-button
              variant="ghost"
              size="lg"
              [fullWidth]="true"
              (buttonClick)="loginWithGoogle()"
              [disabled]="loading()"
            >
              <i nz-icon nzType="google" class="social-icon"></i>
              Google
            </app-button>

            <app-button
              variant="ghost"
              size="lg"
              [fullWidth]="true"
              (buttonClick)="loginWithLinkedIn()"
              [disabled]="loading()"
              class="linkedin-btn"
            >
              <i nz-icon nzType="linkedin" class="social-icon"></i>
              LinkedIn
            </app-button>
          </div>

          <!-- Register Link -->
          <div class="login-footer">
            <p>
              Don't have an account?
              <a routerLink="/auth/register" class="register-link">Sign up</a>
            </p>
          </div>
        </app-card>
      </div>
    </div>
  `,
  styles: [
    `
      .login-container {
        @apply min-h-screen bg-background dark:bg-background-dark;
        @apply flex items-center justify-center px-4 py-8;
      }

      .login-content {
        @apply w-full max-w-md;
      }

      .login-card {
        @apply p-8;
      }

      .login-header {
        @apply text-center mb-8;
      }

      .brand-logo {
        @apply flex items-center justify-center space-x-3 mb-6;
      }

      .brand-icon {
        @apply text-3xl text-indigo-600 dark:text-indigo-400;
      }

      .brand-text {
        @apply text-2xl font-bold text-slate-900 dark:text-slate-100;
      }

      .login-title {
        @apply text-2xl font-bold text-slate-900 dark:text-slate-100 mb-2;
      }

      .login-subtitle {
        @apply text-slate-600 dark:text-slate-400;
      }

      .login-error {
        @apply mb-6;
      }

      .login-form {
        @apply space-y-4;
      }

      .login-options {
        @apply mb-6;
      }

      .login-options-content {
        @apply flex items-center justify-between;
      }

      .forgot-password-link {
        @apply text-indigo-600 hover:text-indigo-700 dark:text-indigo-400 dark:hover:text-indigo-300;
        @apply text-sm font-medium transition-colors duration-200;
      }

      .login-submit {
        @apply mb-6;
      }

      .social-login {
        @apply space-y-3 mb-6;
      }

      .social-icon {
        @apply mr-2;
      }

      .linkedin-btn {
        @apply bg-blue-600 hover:bg-blue-700 text-white border-blue-600 hover:border-blue-700;
      }

      .login-footer {
        @apply text-center text-sm text-slate-600 dark:text-slate-400;
      }

      .register-link {
        @apply text-indigo-600 hover:text-indigo-700 dark:text-indigo-400 dark:hover:text-indigo-300;
        @apply font-medium transition-colors duration-200;
      }

      .password-toggle {
        @apply cursor-pointer text-slate-400 hover:text-slate-600 dark:hover:text-slate-300;
        @apply transition-colors duration-200;
      }

      .error {
        @apply border-red-500 focus:border-red-500 focus:ring-red-500;
      }

      /* Ant Design Form Overrides */
      ::ng-deep .ant-form-item {
        @apply mb-4;
      }

      ::ng-deep .ant-form-item-label > label {
        @apply text-slate-700 dark:text-slate-300 font-medium;
      }

      ::ng-deep .ant-input {
        @apply border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800;
        @apply text-slate-900 dark:text-slate-100;
      }

      ::ng-deep .ant-input:focus {
        @apply border-indigo-500 ring-2 ring-indigo-500 ring-opacity-20;
      }

      ::ng-deep .ant-divider-horizontal.ant-divider-with-text {
        @apply text-slate-600 dark:text-slate-400;
      }
    `,
  ],
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // Signals for reactive state
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly passwordVisible = signal(false);

  // Reactive form
  protected readonly loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    rememberMe: [false],
  });

  constructor() {
    // Clear any existing errors when form values change
    this.loginForm.valueChanges.subscribe(() => {
      if (this.error()) {
        this.error.set(null);
      }
    });

    // Subscribe to auth service errors
    this.authService.error$.subscribe((error) => {
      this.error.set(error);
    });

    // Subscribe to auth service loading state
    this.authService.loading$.subscribe((loading) => {
      this.loading.set(loading);
    });
  }

  protected onSubmit(): void {
    if (this.loginForm.valid) {
      const credentials: LoginData = {
        email: this.loginForm.value.email,
        password: this.loginForm.value.password,
        rememberMe: this.loginForm.value.rememberMe,
      };

      this.authService
        .login(credentials)
        .pipe(
          finalize(() => {
            // Loading state is managed by auth service
          }),
        )
        .subscribe({
          next: () => {
            this.router.navigate(['/dashboard']);
          },
          error: () => {
            // Error is handled by auth service
          },
        });
    } else {
      this.markFormGroupTouched();
    }
  }

  protected loginWithGoogle(): void {
    // Google OAuth integration will be implemented in social auth task
    this.error.set('Social login coming soon');
  }

  protected loginWithLinkedIn(): void {
    // LinkedIn OAuth integration will be implemented in social auth task
    this.error.set('Social login coming soon');
  }

  protected togglePasswordVisibility(): void {
    this.passwordVisible.set(!this.passwordVisible());
  }

  protected isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  protected clearError(): void {
    this.error.set(null);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach((key) => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
  }
}
