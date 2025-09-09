import { Injectable, signal, effect, PLATFORM_ID, Inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly isBrowser: boolean;

  // Theme state using Angular signals
  public readonly isDarkMode = signal<boolean>(false);

  constructor(@Inject(PLATFORM_ID) private readonly platformId: object) {
    this.isBrowser = isPlatformBrowser(this.platformId);

    // Initialize theme on browser
    if (this.isBrowser) {
      this.initializeTheme();
    }

    // React to theme changes
    effect(() => {
      if (this.isBrowser) {
        this.updateTheme(this.isDarkMode());
      }
    });
  }

  private initializeTheme(): void {
    // Check for saved theme preference or default to system preference
    const savedTheme = localStorage.getItem('theme');
    const prefersDark = window.matchMedia(
      '(prefers-color-scheme: dark)',
    ).matches;

    const shouldUseDark = savedTheme === 'dark' || (!savedTheme && prefersDark);
    this.isDarkMode.set(shouldUseDark);

    // Listen for system theme changes
    window
      .matchMedia('(prefers-color-scheme: dark)')
      .addEventListener('change', (e) => {
        if (!localStorage.getItem('theme')) {
          this.isDarkMode.set(e.matches);
        }
      });
  }

  private updateTheme(isDark: boolean): void {
    const htmlElement = document.documentElement;

    if (isDark) {
      htmlElement.classList.add('dark');
      localStorage.setItem('theme', 'dark');
    } else {
      htmlElement.classList.remove('dark');
      localStorage.setItem('theme', 'light');
    }
  }

  /**
   * Toggle between light and dark mode
   */
  public toggleTheme(): void {
    this.isDarkMode.update((current) => !current);
  }

  /**
   * Set specific theme
   */
  public setTheme(isDark: boolean): void {
    this.isDarkMode.set(isDark);
  }

  /**
   * Reset to system preference
   */
  public resetToSystemTheme(): void {
    if (this.isBrowser) {
      localStorage.removeItem('theme');
      const prefersDark = window.matchMedia(
        '(prefers-color-scheme: dark)',
      ).matches;
      this.isDarkMode.set(prefersDark);
    }
  }
}
