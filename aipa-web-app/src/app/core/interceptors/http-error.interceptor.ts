import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unknown error occurred';

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Client Error: ${error.error.message}`;
      } else {
        // Server-side error
        switch (error.status) {
          case 400:
            errorMessage = 'Bad Request: Please check your input';
            break;
          case 401:
            errorMessage = 'Unauthorized: Please log in';
            // TODO: Redirect to login page or refresh token
            break;
          case 403:
            errorMessage = 'Forbidden: You do not have permission';
            break;
          case 404:
            errorMessage = 'Not Found: The requested resource was not found';
            break;
          case 500:
            errorMessage = 'Internal Server Error: Please try again later';
            break;
          case 503:
            errorMessage = 'Service Unavailable: Please try again later';
            break;
          default:
            errorMessage = `Server Error: ${error.status} - ${error.message}`;
        }
      }

      // Log error in development
      if (!environment.production || environment.enableLogging) {
        console.error('HTTP Error Interceptor:', {
          status: error.status,
          message: errorMessage,
          url: error.url,
          error: error.error,
        });
      }

      // TODO: Show user notification using ng-zorro-antd notification service

      return throwError(() => new Error(errorMessage));
    }),
  );
};
