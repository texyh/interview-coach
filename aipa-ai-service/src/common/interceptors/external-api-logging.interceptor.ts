import { Injectable, NestInterceptor, ExecutionContext, CallHandler, Logger } from '@nestjs/common';
import { Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { Request, Response } from 'express';

/**
 * Interceptor for comprehensive logging of external API calls and their performance
 */
@Injectable()
export class ExternalApiLoggingInterceptor implements NestInterceptor {
  private readonly logger = new Logger(ExternalApiLoggingInterceptor.name);

  intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
    const startTime = Date.now();
    const request = context.switchToHttp().getRequest<Request>();
    const response = context.switchToHttp().getResponse<Response>();
    const requestId = this.generateRequestId();

    // Log incoming request for external API endpoints
    if (this.isExternalApiEndpoint(request.path)) {
      this.logIncomingRequest(request, requestId);
    }

    return next.handle().pipe(
      tap((data) => {
        if (this.isExternalApiEndpoint(request.path)) {
          this.logSuccessfulResponse(request, response, data, startTime, requestId);
        }
      }),
      catchError((error) => {
        if (this.isExternalApiEndpoint(request.path)) {
          this.logErrorResponse(request, response, error, startTime, requestId);
        }
        throw error;
      })
    );
  }

  private isExternalApiEndpoint(path: string): boolean {
    const externalApiPaths = ['/openai', '/speech', '/analysis', '/feedback'];

    return externalApiPaths.some((apiPath) => path.includes(apiPath));
  }

  private logIncomingRequest(request: Request, requestId: string): void {
    const method = request.method;
    const url = request.url;
    const headers = request.headers;
    const body = request.body as unknown;
    const query = request.query;

    this.logger.log({
      message: 'Incoming API request',
      requestId,
      method,
      url,
      userAgent: headers['user-agent'],
      contentType: headers['content-type'],
      queryParams: Object.keys(query).length > 0 ? query : undefined,
      bodySize: body ? JSON.stringify(body).length : 0,
      timestamp: new Date().toISOString(),
    });
  }

  private logSuccessfulResponse(
    request: Request,
    response: Response,
    data: any,
    startTime: number,
    requestId: string
  ): void {
    const duration = Date.now() - startTime;
    const responseSize = data ? JSON.stringify(data).length : 0;

    this.logger.log({
      message: 'API request completed successfully',
      requestId,
      method: request.method,
      url: request.url,
      statusCode: response.statusCode,
      duration,
      responseSize,
      performance: this.categorizePerformance(duration),
      timestamp: new Date().toISOString(),
    });

    // Log performance warnings for slow requests
    if (duration > 5000) {
      this.logger.warn({
        message: 'Slow API request detected',
        requestId,
        duration,
        url: request.url,
        method: request.method,
      });
    }
  }

  private logErrorResponse(
    request: Request,
    response: Response,
    error: any,
    startTime: number,
    requestId: string
  ): void {
    const duration = Date.now() - startTime;

    this.logger.error({
      message: 'API request failed',
      requestId,
      method: request.method,
      url: request.url,
      statusCode: response.statusCode || 500,
      duration,
      error: {
        name: error instanceof Error ? error.name : 'Unknown',
        message: error instanceof Error ? error.message : String(error),
        stack: error instanceof Error ? error.stack : undefined,
      },
      timestamp: new Date().toISOString(),
    });
  }

  private categorizePerformance(duration: number): string {
    if (duration < 500) return 'excellent';
    if (duration < 1000) return 'good';
    if (duration < 2000) return 'acceptable';
    if (duration < 5000) return 'slow';
    return 'very_slow';
  }

  private generateRequestId(): string {
    return `req_${Date.now()}_${Math.random().toString(36).substring(2, 9)}`;
  }
}
