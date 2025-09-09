import { Component, inject, signal, OnDestroy } from '@angular/core';
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
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzCardModule } from 'ng-zorro-antd/card';
import { finalize } from 'rxjs/operators';

import { AuthService } from '../../../core/services/auth.service';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    NzFormModule,
    NzInputModule,
    NzButtonModule,
    NzIconModule,
    NzAlertModule,
    NzCardModule,
    ButtonComponent,
    CardComponent,
  ],
  template: `
    <div class="forgot-password-container">
      <div class="forgot-password-content">
        <app-card variant="elevated" class="forgot-password-card">
          <!-- Header -->
          <div class="forgot-password-header">
            <div class="brand-logo">
              <i
                nz-icon
                nzType="robot"
                nzTheme="outline"
                class="brand-icon"
              ></i>
              <span class="brand-text">AIPA</span>
            </div>
            <h1 class="forgot-password-title">Reset Password</h1>
            <p class="forgot-password-subtitle">
              Enter your email address and we'll send you a link to reset your
              password.
            </p>
          </div>

          <!-- Success Alert -->
          <nz-alert
            *ngIf="emailSent()"
            nzType="success"
            nzMessage="Email Sent!"
            nzDescription="Check your email for a password reset link. If you don't see it, check your spam folder."
            nzCloseable
            (nzOnClose)="clearSuccess()"
            class="forgot-password-success"
          >
          </nz-alert>

          <!-- Error Alert -->
          <nz-alert
            *ngIf="error()"
            nzType="error"
            [nzMessage]="error()"
            nzCloseable
            (nzOnClose)="clearError()"
            class="forgot-password-error"
          >
          </nz-alert>

          <!-- Forgot Password Form -->
          <form
            nz-form
            [formGroup]="forgotPasswordForm"
            (ngSubmit)="onSubmit()"
            class="forgot-password-form"
            *ngIf="!emailSent()"
          >
            <!-- Email Field -->
            <nz-form-item>
              <nz-form-label nzRequired nzFor="email"
                >Email Address</nz-form-label
              >
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

            <!-- Submit Button -->
            <nz-form-item class="forgot-password-submit">
              <app-button
                type="submit"
                variant="primary"
                size="lg"
                [fullWidth]="true"
                [disabled]="forgotPasswordForm.invalid || loading()"
              >
                {{ loading() ? 'Sending...' : 'Send Reset Link' }}
              </app-button>
            </nz-form-item>
          </form>

          <!-- Success Actions -->
          <div *ngIf="emailSent()" class="forgot-password-actions">
            <app-button
              variant="outline"
              size="lg"
              [fullWidth]="true"
              (click)="resendEmail()"
              [disabled]="loading() || !canResend()"
              class="resend-button"
            >
              {{
                loading()
                  ? 'Sending...'
                  : canResend()
                    ? 'Resend Email'
                    : 'Resend in ' + resendCountdown() + 's'
              }}
            </app-button>
          </div>

          <!-- Footer Links -->
          <div class="forgot-password-footer">
            <div class="footer-links">
              <a routerLink="/auth/login" class="back-link">
                <i nz-icon nzType="arrow-left" nzTheme="outline"></i>
                Back to Sign In
              </a>
              <a routerLink="/auth/register" class="register-link">
                Don't have an account? Sign up
              </a>
            </div>
          </div>
        </app-card>
      </div>
    </div>
  `,
  styles: [
    `
      .forgot-password-container {
        @apply min-h-screen bg-background dark:bg-background-dark;
        @apply flex items-center justify-center px-4 py-8;
      }

      .forgot-password-content {
        @apply w-full max-w-md;
      }

      .forgot-password-card {
        @apply p-8;
      }

      .forgot-password-header {
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

      .forgot-password-title {
        @apply text-2xl font-bold text-slate-900 dark:text-slate-100 mb-2;
      }

      .forgot-password-subtitle {
        @apply text-slate-600 dark:text-slate-400 text-sm leading-relaxed;
      }

      .forgot-password-success,
      .forgot-password-error {
        @apply mb-6;
      }

      .forgot-password-form {
        @apply space-y-4;
      }

      .forgot-password-submit {
        @apply mb-6;
      }

      .forgot-password-actions {
        @apply mb-6;
      }

      .resend-button {
        @apply mt-4;
      }

      .forgot-password-footer {
        @apply text-center;
      }

      .footer-links {
        @apply space-y-3;
      }

      .back-link,
      .register-link {
        @apply text-indigo-600 hover:text-indigo-700 dark:text-indigo-400 dark:hover:text-indigo-300;
        @apply font-medium transition-colors duration-200 text-sm;
        @apply flex items-center justify-center space-x-1;
      }

      .back-link {
        @apply text-slate-600 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-300;
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
    `,
  ],
})
export class ForgotPasswordComponent implements OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // Signals for reactive state
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly emailSent = signal(false);
  protected readonly resendCountdown = signal(0);
  private resendTimer?: number;

  // Reactive form
  protected readonly forgotPasswordForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  constructor() {
    // Clear any existing errors when form values change
    this.forgotPasswordForm.valueChanges.subscribe(() => {
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

  ngOnDestroy(): void {
    if (this.resendTimer) {
      clearInterval(this.resendTimer);
    }
  }

  protected onSubmit(): void {
    if (this.forgotPasswordForm.valid) {
      const email = this.forgotPasswordForm.value.email;

      this.authService
        .forgotPassword(email)
        .pipe(
          finalize(() => {
            // Loading state is managed by auth service
          }),
        )
        .subscribe({
          next: () => {
            this.emailSent.set(true);
            this.startResendCountdown();
          },
          error: () => {
            // Error is handled by auth service
          },
        });
    } else {
      this.markFormGroupTouched();
    }
  }

  protected resendEmail(): void {
    if (this.canResend() && this.forgotPasswordForm.valid) {
      const email = this.forgotPasswordForm.value.email;

      this.authService
        .forgotPassword(email)
        .pipe(
          finalize(() => {
            // Loading state is managed by auth service
          }),
        )
        .subscribe({
          next: () => {
            this.startResendCountdown();
          },
          error: () => {
            // Error is handled by auth service
          },
        });
    }
  }

  protected canResend(): boolean {
    return this.resendCountdown() === 0;
  }

  protected isFieldInvalid(fieldName: string): boolean {
    const field = this.forgotPasswordForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  protected clearError(): void {
    this.error.set(null);
  }

  protected clearSuccess(): void {
    this.emailSent.set(false);
    if (this.resendTimer) {
      clearInterval(this.resendTimer);
    }
    this.resendCountdown.set(0);
  }

  private startResendCountdown(): void {
    this.resendCountdown.set(60); // 60 seconds countdown

    if (this.resendTimer) {
      clearInterval(this.resendTimer);
    }

    this.resendTimer = window.setInterval(() => {
      const current = this.resendCountdown();
      if (current > 0) {
        this.resendCountdown.set(current - 1);
      } else if (this.resendTimer) {
        clearInterval(this.resendTimer);
      }
    }, 1000);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.forgotPasswordForm.controls).forEach((key) => {
      const control = this.forgotPasswordForm.get(key);
      control?.markAsTouched();
    });
  }
}
