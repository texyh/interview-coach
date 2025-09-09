import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
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
import { NzStepsModule } from 'ng-zorro-antd/steps';
import { finalize } from 'rxjs/operators';

import { AuthService, RegisterData } from '../../../core/services/auth.service';
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
  selector: 'app-register',
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
    NzStepsModule,
    ButtonComponent,
    CardComponent,
  ],
  template: `
    <div class="register-container">
      <div class="register-content">
        <app-card variant="elevated" class="register-card">
          <!-- Header -->
          <div class="register-header">
            <div class="brand-logo">
              <i
                nz-icon
                nzType="robot"
                nzTheme="outline"
                class="brand-icon"
              ></i>
              <span class="brand-text">AIPA</span>
            </div>
            <h1 class="register-title">Create Account</h1>
            <p class="register-subtitle">
              Start your AI interview preparation journey
            </p>
          </div>

          <!-- Success Alert -->
          <nz-alert
            *ngIf="registrationSuccess()"
            nzType="success"
            nzMessage="Registration Successful!"
            nzDescription="Please check your email to verify your account before signing in."
            nzCloseable
            (nzOnClose)="clearSuccess()"
            class="register-success"
          >
          </nz-alert>

          <!-- Error Alert -->
          <nz-alert
            *ngIf="error()"
            nzType="error"
            [nzMessage]="error()"
            nzCloseable
            (nzOnClose)="clearError()"
            class="register-error"
          >
          </nz-alert>

          <!-- Registration Form -->
          <form
            nz-form
            [formGroup]="registerForm"
            (ngSubmit)="onSubmit()"
            class="register-form"
          >
            <nz-row [nzGutter]="16">
              <!-- First Name -->
              <nz-col [nzSpan]="12">
                <nz-form-item>
                  <nz-form-label nzRequired nzFor="firstName"
                    >First Name</nz-form-label
                  >
                  <nz-form-control
                    nzHasFeedback
                    [nzErrorTip]="firstNameErrorTip"
                  >
                    <input
                      nz-input
                      formControlName="firstName"
                      id="firstName"
                      placeholder="First name"
                      [class.error]="isFieldInvalid('firstName')"
                    />
                    <ng-template #firstNameErrorTip let-control>
                      <span *ngIf="control.errors?.['required']"
                        >First name is required</span
                      >
                    </ng-template>
                  </nz-form-control>
                </nz-form-item>
              </nz-col>

              <!-- Last Name -->
              <nz-col [nzSpan]="12">
                <nz-form-item>
                  <nz-form-label nzRequired nzFor="lastName"
                    >Last Name</nz-form-label
                  >
                  <nz-form-control
                    nzHasFeedback
                    [nzErrorTip]="lastNameErrorTip"
                  >
                    <input
                      nz-input
                      formControlName="lastName"
                      id="lastName"
                      placeholder="Last name"
                      [class.error]="isFieldInvalid('lastName')"
                    />
                    <ng-template #lastNameErrorTip let-control>
                      <span *ngIf="control.errors?.['required']"
                        >Last name is required</span
                      >
                    </ng-template>
                  </nz-form-control>
                </nz-form-item>
              </nz-col>
            </nz-row>

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
                    placeholder="Confirm your password"
                    [class.error]="
                      isFieldInvalid('confirmPassword') ||
                      registerForm.errors?.['passwordMismatch']
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
                      registerForm.errors?.['passwordMismatch'] &&
                      !control.errors?.['required']
                    "
                  >
                    Passwords do not match
                  </span>
                </ng-template>
              </nz-form-control>
            </nz-form-item>

            <!-- Terms and Conditions -->
            <nz-form-item class="terms-checkbox">
              <nz-form-control [nzErrorTip]="termsErrorTip">
                <label nz-checkbox formControlName="acceptTerms">
                  I agree to the
                  <a href="/terms" target="_blank" class="terms-link"
                    >Terms of Service</a
                  >
                  and
                  <a href="/privacy" target="_blank" class="terms-link"
                    >Privacy Policy</a
                  >
                </label>
                <ng-template #termsErrorTip let-control>
                  <span *ngIf="control.errors?.['required']">
                    You must accept the terms and conditions
                  </span>
                </ng-template>
              </nz-form-control>
            </nz-form-item>

            <!-- Submit Button -->
            <nz-form-item class="register-submit">
              <app-button
                type="submit"
                variant="primary"
                size="lg"
                [fullWidth]="true"
                [disabled]="registerForm.invalid || loading()"
              >
                {{ loading() ? 'Creating Account...' : 'Create Account' }}
              </app-button>
            </nz-form-item>
          </form>

          <!-- Login Link -->
          <div class="register-footer">
            <p>
              Already have an account?
              <a routerLink="/auth/login" class="login-link">Sign in</a>
            </p>
          </div>
        </app-card>
      </div>
    </div>
  `,
  styles: [
    `
      .register-container {
        @apply min-h-screen bg-background dark:bg-background-dark;
        @apply flex items-center justify-center px-4 py-8;
      }

      .register-content {
        @apply w-full max-w-lg;
      }

      .register-card {
        @apply p-8;
      }

      .register-header {
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

      .register-title {
        @apply text-2xl font-bold text-slate-900 dark:text-slate-100 mb-2;
      }

      .register-subtitle {
        @apply text-slate-600 dark:text-slate-400;
      }

      .register-success,
      .register-error {
        @apply mb-6;
      }

      .register-form {
        @apply space-y-4;
      }

      .terms-checkbox {
        @apply mb-6;
      }

      .terms-link {
        @apply text-indigo-600 hover:text-indigo-700 dark:text-indigo-400 dark:hover:text-indigo-300;
        @apply font-medium transition-colors duration-200;
      }

      .register-submit {
        @apply mb-6;
      }

      .register-footer {
        @apply text-center text-sm text-slate-600 dark:text-slate-400;
      }

      .login-link {
        @apply text-indigo-600 hover:text-indigo-700 dark:text-indigo-400 dark:hover:text-indigo-300;
        @apply font-medium transition-colors duration-200;
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

      ::ng-deep .ant-checkbox-wrapper {
        @apply text-slate-700 dark:text-slate-300;
      }
    `,
  ],
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // Signals for reactive state
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly registrationSuccess = signal(false);
  protected readonly passwordVisible = signal(false);
  protected readonly confirmPasswordVisible = signal(false);

  // Reactive form
  protected readonly registerForm: FormGroup = this.fb.group(
    {
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, strongPasswordValidator]],
      confirmPassword: ['', [Validators.required]],
      acceptTerms: [false, [Validators.requiredTrue]],
    },
    { validators: passwordMatchValidator },
  );

  constructor() {
    // Clear any existing errors when form values change
    this.registerForm.valueChanges.subscribe(() => {
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
    if (this.registerForm.valid) {
      const registerData: RegisterData = {
        email: this.registerForm.value.email,
        password: this.registerForm.value.password,
        confirmPassword: this.registerForm.value.confirmPassword,
        firstName: this.registerForm.value.firstName,
        lastName: this.registerForm.value.lastName,
        acceptTerms: this.registerForm.value.acceptTerms,
      };

      this.authService
        .register(registerData)
        .pipe(
          finalize(() => {
            // Loading state is managed by auth service
          }),
        )
        .subscribe({
          next: () => {
            this.registrationSuccess.set(true);
            this.registerForm.reset();
            // Redirect to login after successful registration
            setTimeout(() => {
              this.router.navigate(['/auth/login'], {
                queryParams: { message: 'registration-success' },
              });
            }, 3000);
          },
          error: () => {
            // Error is handled by auth service
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
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  protected clearError(): void {
    this.error.set(null);
  }

  protected clearSuccess(): void {
    this.registrationSuccess.set(false);
  }

  // Password strength validation helpers
  protected hasUpperCase(): boolean {
    const password = this.registerForm.get('password')?.value || '';
    return /[A-Z]/.test(password);
  }

  protected hasLowerCase(): boolean {
    const password = this.registerForm.get('password')?.value || '';
    return /[a-z]/.test(password);
  }

  protected hasNumber(): boolean {
    const password = this.registerForm.get('password')?.value || '';
    return /\d/.test(password);
  }

  protected hasSpecialChar(): boolean {
    const password = this.registerForm.get('password')?.value || '';
    return /[!@#$%^&*(),.?":{}|<>]/.test(password);
  }

  protected hasMinLength(): boolean {
    const password = this.registerForm.get('password')?.value || '';
    return password.length >= 8;
  }

  private markFormGroupTouched(): void {
    Object.keys(this.registerForm.controls).forEach((key) => {
      const control = this.registerForm.get(key);
      control?.markAsTouched();
    });
  }
}
