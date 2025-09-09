// import { ErrorHandler, Injectable } from '@angular/core';
// import { environment } from '../../../environments/environment';

// @Injectable({
//   providedIn: 'root',
// })
// export class GlobalErrorHandler implements ErrorHandler {
//   handleError(error: any): void {
//     // Log error to console in development
//     if (!environment.production || environment.enableLogging) {
//       console.error('Global Error Handler:', error);
//     }

//     // Here you can add error reporting to external services
//     // e.g., Sentry, LogRocket, etc.

//     // Extract useful error information
//     const errorInfo = {
//       message: error?.message || 'Unknown error occurred',
//       stack: error?.stack,
//       timestamp: new Date().toISOString(),
//       url: window.location.href,
//       userAgent: navigator.userAgent,
//     };

//     // Send error to monitoring service in production
//     if (environment.production) {
//       this.reportError(errorInfo);
//     }

//     // Show user-friendly error message
//     this.showUserError(errorInfo.message);
//   }

//   private reportError(errorInfo: any): void {
//     // TODO: Implement error reporting to monitoring service
//     // Example: Send to Sentry, LogRocket, or custom endpoint
//     console.warn('Error reporting not yet implemented:', errorInfo);
//   }

//   private showUserError(message: string): void {
//     // TODO: Show user-friendly error notification
//     // This could use ng-zorro-antd notification service
//     console.warn('User error notification not yet implemented:', message);
//   }
// }
