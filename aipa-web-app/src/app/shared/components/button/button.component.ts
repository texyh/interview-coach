import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'outline';
export type ButtonSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      [class]="buttonClasses"
      [disabled]="disabled"
      [type]="type"
      (click)="handleClick($event)"
    >
      <ng-content></ng-content>
    </button>
  `,
  styles: [
    `
      button {
        @apply font-medium transition-all duration-200 ease-in-out;
        @apply focus:outline-none focus:ring-2 focus:ring-offset-2;
        @apply disabled:opacity-50 disabled:cursor-not-allowed;
      }
    `,
  ],
})
export class ButtonComponent {
  @Input() variant: ButtonVariant = 'primary';
  @Input() size: ButtonSize = 'md';
  @Input() disabled: boolean = false;
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() fullWidth: boolean = false;

  @Output() buttonClick = new EventEmitter<Event>();

  get buttonClasses(): string {
    const baseClasses =
      'inline-flex items-center justify-center border border-transparent rounded-lg';

    const sizeClasses = {
      sm: 'px-3 py-1.5 text-sm',
      md: 'px-4 py-2 text-sm',
      lg: 'px-6 py-3 text-base',
    };

    const variantClasses = {
      primary:
        'bg-primary-600 hover:bg-primary-700 text-white shadow-soft hover:shadow-medium focus:ring-primary-500',
      secondary:
        'bg-white hover:bg-slate-50 text-slate-700 border-slate-200 shadow-soft hover:shadow-medium focus:ring-slate-500',
      ghost:
        'hover:bg-slate-100 text-slate-600 hover:text-slate-800 focus:ring-slate-500',
      outline:
        'border-slate-300 text-slate-700 hover:bg-slate-50 focus:ring-slate-500',
    };

    const widthClass = this.fullWidth ? 'w-full' : '';

    return [
      baseClasses,
      sizeClasses[this.size],
      variantClasses[this.variant],
      widthClass,
    ]
      .filter(Boolean)
      .join(' ');
  }

  handleClick(event: Event): void {
    if (!this.disabled) {
      this.buttonClick.emit(event);
    }
  }
}
