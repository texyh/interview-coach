import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type CardVariant = 'default' | 'bordered' | 'elevated';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="cardClasses">
      <!-- Card header -->
      <div *ngIf="title" class="card-header">
        <h3 class="card-title">{{ title }}</h3>
        <ng-content select="[slot=header]"></ng-content>
      </div>

      <!-- Card content -->
      <div class="card-content">
        <ng-content></ng-content>
      </div>

      <!-- Card footer -->
      <div class="card-footer">
        <ng-content select="[slot=footer]"></ng-content>
      </div>
    </div>
  `,
  styles: [
    `
      .card-header {
        @apply p-6 pb-0;
      }

      .card-title {
        @apply text-lg font-semibold text-slate-900 dark:text-slate-100;
      }

      .card-content {
        @apply p-6;
      }

      .card-footer {
        @apply px-6 pb-6 pt-0;
      }

      /* Ensure no padding if no header */
      .card-content:first-child {
        @apply pt-6;
      }

      /* Ensure no padding if no footer */
      .card-content:last-child {
        @apply pb-6;
      }
    `,
  ],
})
export class CardComponent {
  @Input() title?: string;
  @Input() variant: CardVariant = 'default';
  @Input() padding: boolean = true;
  @Input() hoverable: boolean = false;

  get cardClasses(): string {
    const baseClasses =
      'bg-white dark:bg-card-dark rounded-2xl transition-all duration-300 ease-in-out';

    const variantClasses = {
      default: 'border border-slate-200 dark:border-border-dark',
      bordered: 'border-2 border-slate-200 dark:border-border-dark',
      elevated:
        'shadow-medium hover:shadow-large border border-slate-200 dark:border-border-dark',
    };

    const interactionClasses = this.hoverable
      ? 'hover:shadow-medium cursor-pointer'
      : '';
    const paddingClasses = this.padding ? '' : 'p-0';

    return [
      baseClasses,
      variantClasses[this.variant],
      interactionClasses,
      paddingClasses,
    ]
      .filter(Boolean)
      .join(' ');
  }
}
