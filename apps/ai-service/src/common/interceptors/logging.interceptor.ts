import { Injectable, NestInterceptor, ExecutionContext, CallHandler, Logger } from '@nestjs/common';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

/**
 * Interceptor for logging HTTP requests and responses
 */
@Injectable()
export class LoggingInterceptor implements NestInterceptor {
  private readonly logger = new Logger(LoggingInterceptor.name);

  intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
    const request = context.switchToHttp().getRequest();
    const { method, url, body } = request;
    const startTime = Date.now();

    this.logger.log(`Incoming Request: ${method} ${url}`);

    if (Object.keys(body || {}).length > 0) {
      this.logger.debug(`Request Body: ${JSON.stringify(body)}`);
    }

    return next.handle().pipe(
      tap({
        next: (response) => {
          const duration = Date.now() - startTime;
          this.logger.log(`Outgoing Response: ${method} ${url} - ${duration}ms`);
          this.logger.debug(`Response: ${JSON.stringify(response)}`);
        },
        error: (error) => {
          const duration = Date.now() - startTime;
          this.logger.error(`Error Response: ${method} ${url} - ${duration}ms`, error);
        },
      })
    );
  }
}
