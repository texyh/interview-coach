import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzTooltipModule } from 'ng-zorro-antd/tooltip';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [CommonModule, NzIconModule, NzButtonModule, NzTooltipModule],
  template: `
    <button
      nz-button
      nzType="text"
      nzShape="circle"
      nzSize="large"
      nz-tooltip
      [nzTooltipTitle]="
        isDarkMode() ? 'Switch to light mode' : 'Switch to dark mode'
      "
      (click)="handleToggleTheme()"
      class="theme-toggle-btn"
    >
      <i
        nz-icon
        [nzType]="isDarkMode() ? 'sun' : 'moon'"
        nzTheme="outline"
        class="text-lg"
      ></i>
    </button>
  `,
  styles: [
    `
      .theme-toggle-btn {
        @apply text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-100;
        @apply transition-colors duration-200;
      }

      .theme-toggle-btn:hover {
        @apply bg-slate-100 dark:bg-slate-800;
      }
    `,
  ],
})
export class ThemeToggleComponent {
  private readonly themeService = inject(ThemeService);

  // Expose the signal for template use
  readonly isDarkMode = this.themeService.isDarkMode;

  handleToggleTheme(): void {
    this.themeService.toggleTheme();
  }
}
