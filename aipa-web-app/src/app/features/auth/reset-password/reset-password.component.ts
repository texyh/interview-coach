import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
} from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
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

// Custom Validators
function passwordMatchValidator(
  control: AbstractControl,
): { [key: string]: boolean } | null {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');

  if (password && confirmPassword && password.value !== confirmPassword.value) {
    return { passwordMismatch: true };
  }
  return null;
}

function strongPasswordValidator(
  control: AbstractControl,
): { [key: string]: boolean } | null {
  const value = control.value;
  if (!value) return null;

  const hasUpperCase = /[A-Z]/.test(value);
  const hasLowerCase = /[a-z]/.test(value);
  const hasNumber = /\d/.test(value);
  const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(value);
  const minLength = value.length >= 8;

  if (
    !hasUpperCase ||
    !hasLowerCase ||
    !hasNumber ||
    !hasSpecialChar ||
    !minLength
  ) {
    return { strongPassword: true };
  }
  return null;
}

@Component({
  selector: 'app-reset-password',
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
    <div class="reset-password-container">
      <div class="reset-password-content">
        <app-card variant="elevated" class="reset-password-card">
          <!-- Header -->
          <div class="reset-password-header">
            <div class="brand-logo">
              <i
                nz-icon
                nzType="robot"
                nzTheme="outline"
                class="brand-icon"
              ></i>
              <span class="brand-text">AIPA</span>
            </div>
            <h1 class="reset-password-title">Reset Password</h1>
            <p class="reset-password-subtitle">
              Enter your new password below.
            </p>
          </div>

          <!-- Invalid Token Alert -->
          <nz-alert
            *ngIf="invalidToken()"
            nzType="error"
            nzMessage="Invalid Reset Link"
            nzDescription="This password reset link is invalid or has expired. Please request a new one."
            nzCloseable
            (nzOnClose)="clearInvalidToken()"
            class="reset-password-error"
          >
          </nz-alert>

          <!-- Success Alert -->
          <nz-alert
            *ngIf="resetSuccess()"
            nzType="success"
            nzMessage="Password Reset Successful!"
            nzDescription="Your password has been successfully reset. You can now sign in with your new password."
            nzCloseable
            (nzOnClose)="clearSuccess()"
            class="reset-password-success"
          >
          </nz-alert>

          <!-- Error Alert -->
          <nz-alert
            *ngIf="error()"
            nzType="error"
            [nzMessage]="error()"
            nzCloseable
            (nzOnClose)="clearError()"
            class="reset-password-error"
          >
          </nz-alert>

          <!-- Reset Password Form -->
          <form
            nz-form
            [formGroup]="resetPasswordForm"
            (ngSubmit)="onSubmit()"
            class="reset-password-form"
            *ngIf="!resetSuccess() && !invalidToken()"
          >
            <!-- Password Field -->
            <nz-form-item>
              <nz-form-label nzRequired nzFor="password"
                >New Password</nz-form-label
              >
              <nz-form-control nzHasFeedback [nzErrorTip]="passwordErrorTip">
                <nz-input-group [nzSuffix]="passwordSuffixTemplate">
                  <input
                    nz-input
                    formControlName="password"
                    id="password"
                    [type]="passwordVisible() ? 'text' : 'password'"
                    placeholder="Create a strong password"
                    [class.error]="isFieldInvalid('password')"
                  />
                </nz-input-group>
                <ng-template #passwordSuffixTemplate>
                  <i
                    nz-icon
                    [nzType]="passwordVisible() ? 'eye-invisible' : 'eye'"
                    (click)="togglePasswordVisibility()"
                    class="password-toggle"
                  >
                  </i>
                </ng-template>
                <ng-template #passwordErrorTip let-control>
                  <div *ngIf="control.errors?.['required']">
                    Password is required
                  </div>
                  <div
                    *ngIf="control.errors?.['strongPassword']"
                    class="password-requirements"
                  >
                    <p>Password must contain:</p>
                    <ul>
                      <li [class.valid]="hasUpperCase()">
                        At least one uppercase letter
                      </li>
                      <li [class.valid]="hasLowerCase()">
                        At least one lowercase letter
                      </li>
                      <li [class.valid]="hasNumber()">At least one number</li>
                      <li [class.valid]="hasSpecialChar()">
                        At least one special character
                      </li>
                      <li [class.valid]="hasMinLength()">
                        At least 8 characters
                      </li>
                    </ul>
                  </div>
                </ng-template>
              </nz-form-control>
            </nz-form-item>

            <!-- Confirm Password Field -->
            <nz-form-item>
              <nz-form-label nzRequired nzFor="confirmPassword"
                >Confirm Password</nz-form-label
              >
              <nz-form-control
                nzHasFeedback
                [nzErrorTip]="confirmPasswordErrorTip"
              >
                <nz-input-group [nzSuffix]="confirmPasswordSuffixTemplate">
                  <input
                    nz-input
                    formControlName="confirmPassword"
                    id="confirmPassword"
                    [type]="confirmPasswordVisible() ? 'text' : 'password'"
                    placeholder="Confirm your new password"
                    [class.error]="
                      isFieldInvalid('confirmPassword') ||
                      resetPasswordForm.errors?.['passwordMismatch']
                    "
                  />
                </nz-input-group>
                <ng-template #confirmPasswordSuffixTemplate>
                  <i
                    nz-icon
                    [nzType]="
                      confirmPasswordVisible() ? 'eye-invisible' : 'eye'
                    "
                    (click)="toggleConfirmPasswordVisibility()"
                    class="password-toggle"
                  >
                  </i>
                </ng-template>
                <ng-template #confirmPasswordErrorTip let-control>
                  <span *ngIf="control.errors?.['required']"
                    >Please confirm your password</span
                  >
                  <span
                    *ngIf="
                      resetPasswordForm.errors?.['passwordMismatch'] &&
                      !control.errors?.['required']
                    "
                  >
                    Passwords do not match
                  </span>
                </ng-template>
              </nz-form-control>
            </nz-form-item>

            <!-- Submit Button -->
            <nz-form-item class="reset-password-submit">
              <app-button
                type="submit"
                variant="primary"
                size="lg"
                [fullWidth]="true"
                [disabled]="resetPasswordForm.invalid || loading()"
              >
                {{ loading() ? 'Resetting...' : 'Reset Password' }}
              </app-button>
            </nz-form-item>
          </form>

          <!-- Success Actions -->
          <div *ngIf="resetSuccess()" class="reset-password-actions">
            <app-button
              variant="primary"
              size="lg"
              [fullWidth]="true"
              routerLink="/auth/login"
              class="sign-in-button"
            >
              Sign In
            </app-button>
          </div>

          <!-- Invalid Token Actions -->
          <div *ngIf="invalidToken()" class="reset-password-actions">
            <app-button
              variant="primary"
              size="lg"
              [fullWidth]="true"
              routerLink="/auth/forgot-password"
              class="request-new-button"
            >
              Request New Reset Link
            </app-button>
          </div>

          <!-- Footer Links -->
          <div class="reset-password-footer">
            <a routerLink="/auth/login" class="back-link">
              <i nz-icon nzType="arrow-left" nzTheme="outline"></i>
              Back to Sign In
            </a>
          </div>
        </app-card>
      </div>
    </div>
  `,
  styles: [
    `
      .reset-password-container {
        @apply min-h-screen bg-background dark:bg-background-dark;
        @apply flex items-center justify-center px-4 py-8;
      }

      .reset-password-content {
        @apply w-full max-w-md;
      }

      .reset-password-card {
        @apply p-8;
      }

      .reset-password-header {
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

      .reset-password-title {
        @apply text-2xl font-bold text-slate-900 dark:text-slate-100 mb-2;
      }

      .reset-password-subtitle {
        @apply text-slate-600 dark:text-slate-400 text-sm;
      }

      .reset-password-success,
      .reset-password-error {
        @apply mb-6;
      }

      .reset-password-form {
        @apply space-y-4;
      }

      .reset-password-submit {
        @apply mb-6;
      }

      .reset-password-actions {
        @apply mb-6;
      }

      .reset-password-footer {
        @apply text-center;
      }

      .back-link {
        @apply text-slate-600 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-300;
        @apply font-medium transition-colors duration-200 text-sm;
        @apply flex items-center justify-center space-x-1;
      }

      .password-toggle {
        @apply cursor-pointer text-slate-400 hover:text-slate-600 dark:hover:text-slate-300;
        @apply transition-colors duration-200;
      }

      .password-requirements {
        @apply mt-2;
      }

      .password-requirements p {
        @apply text-sm font-medium text-slate-700 dark:text-slate-300 mb-2;
      }

      .password-requirements ul {
        @apply list-none space-y-1;
      }

      .password-requirements li {
        @apply text-sm text-red-600 dark:text-red-400;
        @apply flex items-center space-x-1;
      }

      .password-requirements li.valid {
        @apply text-green-600 dark:text-green-400;
      }

      .password-requirements li::before {
        content: '✗';
        @apply text-red-600 dark:text-red-400;
      }

      .password-requirements li.valid::before {
        content: '✓';
        @apply text-green-600 dark:text-green-400;
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
export class ResetPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  // Signals for reactive state
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly resetSuccess = signal(false);
  protected readonly invalidToken = signal(false);
  protected readonly passwordVisible = signal(false);
  protected readonly confirmPasswordVisible = signal(false);

  // Route parameters
  private token = '';
  private email = '';

  // Reactive form
  protected readonly resetPasswordForm: FormGroup = this.fb.group(
    {
      password: ['', [Validators.required, strongPasswordValidator]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordMatchValidator },
  );

  constructor() {
    // Clear any existing errors when form values change
    this.resetPasswordForm.valueChanges.subscribe(() => {
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

  ngOnInit(): void {
    // Get token and email from query parameters
    this.route.queryParams.subscribe((params) => {
      this.token = params['token'] || '';
      this.email = params['email'] || '';

      if (!this.token || !this.email) {
        this.invalidToken.set(true);
      }
    });
  }

  protected onSubmit(): void {
    if (this.resetPasswordForm.valid && this.token && this.email) {
      const password = this.resetPasswordForm.value.password;

      this.authService
        .resetPassword(this.token, this.email, password)
        .pipe(
          finalize(() => {
            // Loading state is managed by auth service
          }),
        )
        .subscribe({
          next: () => {
            this.resetSuccess.set(true);
            this.resetPasswordForm.reset();
          },
          error: (error) => {
            // Check if it's an invalid token error
            if (
              error?.message?.includes('invalid') ||
              error?.message?.includes('expired')
            ) {
              this.invalidToken.set(true);
            }
          },
        });
    } else {
      this.markFormGroupTouched();
    }
  }

  protected togglePasswordVisibility(): void {
    this.passwordVisible.set(!this.passwordVisible());
  }

  protected toggleConfirmPasswordVisibility(): void {
    this.confirmPasswordVisible.set(!this.confirmPasswordVisible());
  }

  protected isFieldInvalid(fieldName: string): boolean {
    const field = this.resetPasswordForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  protected clearError(): void {
    this.error.set(null);
  }

  protected clearSuccess(): void {
    this.resetSuccess.set(false);
  }

  protected clearInvalidToken(): void {
    this.invalidToken.set(false);
  }

  // Password strength validation helpers
  protected hasUpperCase(): boolean {
    const password = this.resetPasswordForm.get('password')?.value || '';
    return /[A-Z]/.test(password);
  }

  protected hasLowerCase(): boolean {
    const password = this.resetPasswordForm.get('password')?.value || '';
    return /[a-z]/.test(password);
  }

  protected hasNumber(): boolean {
    const password = this.resetPasswordForm.get('password')?.value || '';
    return /\d/.test(password);
  }

  protected hasSpecialChar(): boolean {
    const password = this.resetPasswordForm.get('password')?.value || '';
    return /[!@#$%^&*(),.?":{}|<>]/.test(password);
  }

  protected hasMinLength(): boolean {
    const password = this.resetPasswordForm.get('password')?.value || '';
    return password.length >= 8;
  }

  private markFormGroupTouched(): void {
    Object.keys(this.resetPasswordForm.controls).forEach((key) => {
      const control = this.resetPasswordForm.get(key);
      control?.markAsTouched();
    });
  }
}
