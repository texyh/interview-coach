import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { CardComponent } from '../../shared/components/card/card.component';
import { ButtonComponent } from '../../shared/components/button/button.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, NzIconModule, CardComponent, ButtonComponent],
  template: `
    <div class="home-container">
      <!-- Hero Section -->
      <section class="hero-section">
        <app-card variant="elevated" class="hero-card">
          <div class="text-center space-y-8">
            <div class="space-y-4">
              <h1 class="hero-title">Master Your Next Interview</h1>
              <p class="hero-subtitle">
                AI-powered interview preparation that adapts to your needs.
                Practice with confidence and land your dream job.
              </p>
            </div>

            <div class="hero-actions">
              <app-button
                variant="primary"
                size="lg"
                (buttonClick)="handleStartPractice()"
              >
                Start Practicing
              </app-button>
              <app-button
                variant="secondary"
                size="lg"
                (buttonClick)="handleViewDemo()"
              >
                View Demo
              </app-button>
            </div>
          </div>
        </app-card>
      </section>

      <!-- Features Section -->
      <section class="features-section">
        <div class="section-header">
          <h2 class="section-title">Everything You Need to Succeed</h2>
          <p class="section-subtitle">
            Comprehensive tools designed to give you the confidence to excel in
            any interview
          </p>
        </div>

        <div class="features-grid">
          <!-- AI Mock Interviews -->
          <app-card variant="default" [hoverable]="true" class="feature-card">
            <div class="feature-content">
              <div class="feature-icon bg-indigo-100 text-indigo-600">
                <i
                  nz-icon
                  nzType="robot"
                  nzTheme="outline"
                  class="text-2xl"
                ></i>
              </div>
              <h3 class="feature-title">AI Mock Interviews</h3>
              <p class="feature-description">
                Practice with our advanced AI that simulates real interview
                scenarios and provides instant feedback on your responses.
              </p>
            </div>
          </app-card>

          <!-- Question Bank -->
          <app-card variant="default" [hoverable]="true" class="feature-card">
            <div class="feature-content">
              <div class="feature-icon bg-emerald-100 text-emerald-600">
                <i
                  nz-icon
                  nzType="question-circle"
                  nzTheme="outline"
                  class="text-2xl"
                ></i>
              </div>
              <h3 class="feature-title">Expert Question Bank</h3>
              <p class="feature-description">
                Access thousands of curated questions from top companies,
                organized by role, industry, and difficulty level.
              </p>
            </div>
          </app-card>

          <!-- Performance Analytics -->
          <app-card variant="default" [hoverable]="true" class="feature-card">
            <div class="feature-content">
              <div class="feature-icon bg-amber-100 text-amber-600">
                <i
                  nz-icon
                  nzType="bar-chart"
                  nzTheme="outline"
                  class="text-2xl"
                ></i>
              </div>
              <h3 class="feature-title">Performance Analytics</h3>
              <p class="feature-description">
                Track your progress with detailed analytics and personalized
                recommendations to improve your interview skills.
              </p>
            </div>
          </app-card>
        </div>
      </section>

      <!-- Quick Start Section -->
      <section class="quick-start-section">
        <app-card variant="bordered" class="quick-start-card">
          <div class="quick-start-content">
            <div class="quick-start-text">
              <h3 class="quick-start-title">Ready to get started?</h3>
              <p class="quick-start-description">
                Begin your interview preparation journey in just a few clicks.
                No setup required.
              </p>
            </div>
            <div class="quick-start-action">
              <app-button
                variant="primary"
                size="lg"
                [fullWidth]="false"
                (buttonClick)="handleQuickStart()"
              >
                Get Started Free
              </app-button>
            </div>
          </div>
        </app-card>
      </section>
    </div>
  `,
  styles: [
    `
      .home-container {
        @apply max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-16;
      }

      /* Hero Section */
      .hero-section {
        @apply py-8;
      }

      .hero-card {
        @apply p-8 md:p-12;
      }

      .hero-title {
        @apply text-4xl md:text-5xl font-bold text-slate-900 dark:text-slate-100;
        @apply leading-tight;
      }

      .hero-subtitle {
        @apply text-lg md:text-xl text-slate-600 dark:text-slate-400;
        @apply max-w-3xl mx-auto leading-relaxed;
      }

      .hero-actions {
        @apply flex flex-col sm:flex-row gap-4 justify-center items-center;
      }

      /* Features Section */
      .features-section {
        @apply space-y-12;
      }

      .section-header {
        @apply text-center space-y-4;
      }

      .section-title {
        @apply text-3xl font-bold text-slate-900 dark:text-slate-100;
      }

      .section-subtitle {
        @apply text-lg text-slate-600 dark:text-slate-400 max-w-2xl mx-auto;
      }

      .features-grid {
        @apply grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8;
      }

      .feature-card {
        @apply transform transition-all duration-300 hover:scale-105;
      }

      .feature-content {
        @apply text-center space-y-6 p-6;
      }

      .feature-icon {
        @apply w-16 h-16 rounded-2xl flex items-center justify-center mx-auto;
        @apply transition-transform duration-300;
      }

      .feature-card:hover .feature-icon {
        @apply scale-110;
      }

      .feature-title {
        @apply text-xl font-semibold text-slate-900 dark:text-slate-100;
      }

      .feature-description {
        @apply text-slate-600 dark:text-slate-400 leading-relaxed;
      }

      /* Quick Start Section */
      .quick-start-section {
        @apply pb-8;
      }

      .quick-start-card {
        @apply bg-gradient-to-r from-slate-50 to-indigo-50;
        @apply dark:from-slate-800 dark:to-slate-700;
      }

      .quick-start-content {
        @apply flex flex-col lg:flex-row items-center justify-between;
        @apply p-8 gap-8;
      }

      .quick-start-text {
        @apply text-center lg:text-left space-y-3;
      }

      .quick-start-title {
        @apply text-2xl font-bold text-slate-900 dark:text-slate-100;
      }

      .quick-start-description {
        @apply text-slate-600 dark:text-slate-400;
      }

      .quick-start-action {
        @apply flex-shrink-0;
      }

      /* Responsive adjustments */
      @media (max-width: 640px) {
        .hero-card {
          @apply p-6;
        }

        .hero-title {
          @apply text-3xl;
        }

        .hero-subtitle {
          @apply text-base;
        }
      }
    `,
  ],
})
export class HomeComponent {
  handleStartPractice(): void {
    // Navigate to practice questions
  }

  handleViewDemo(): void {
    // Show demo or navigate to demo page
  }

  handleQuickStart(): void {
    // Navigate to onboarding flow
  }
}
