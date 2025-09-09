import { Injectable, Logger } from '@nestjs/common';
import { HttpService } from '@nestjs/axios';
import { ConfigService } from '@nestjs/config';
import { AxiosRequestConfig, AxiosResponse } from 'axios';
import { firstValueFrom } from 'rxjs';

/**
 * Simple HTTP client service with retry logic for external API calls
 */
@Injectable()
export class HttpClientService {
  private readonly logger = new Logger(HttpClientService.name);

  constructor(
    private readonly httpService: HttpService,
    private readonly configService: ConfigService
  ) {}

  /**
   * Make HTTP request with retry logic
   */
  async request<T = any>(
    config: AxiosRequestConfig,
    serviceKey: string,
    maxRetries: number = 3
  ): Promise<AxiosResponse<T>> {
    const requestId = this.generateRequestId();
    const startTime = Date.now();

    this.logRequest(requestId, serviceKey, config);

    for (let attempt = 0; attempt <= maxRetries; attempt++) {
      try {
        const response = await firstValueFrom(this.httpService.request<T>(config));
        this.logSuccess(requestId, serviceKey, response, startTime);
        return response;
      } catch (error) {
        if (attempt === maxRetries) {
          this.logError(requestId, serviceKey, error, startTime);
          throw error;
        }

        if (!this.shouldRetry(error)) {
          this.logError(requestId, serviceKey, error, startTime);
          throw error;
        }

        const delay = this.calculateDelay(attempt);
        this.logger.warn(
          `Request ${requestId} failed, retrying ${attempt + 1}/${maxRetries} after ${delay}ms`
        );
        await this.delay(delay);
      }
    }

    throw new Error('Max retries exceeded');
  }

  private shouldRetry(error: any): boolean {
    // Retry on 5xx errors or network errors
    const errorObj = error as Record<string, any>;
    let status: number | undefined;
    if (
      errorObj?.response &&
      typeof errorObj.response === 'object' &&
      'status' in errorObj.response
    ) {
      const responseObj = errorObj.response as Record<string, any>;
      status =
        typeof responseObj.status === 'number' ? responseObj.status : Number(responseObj.status);
    }

    if (status && status >= 500) return true;
    if (status === 429) return true; // Rate limit

    // Retry on network errors
    const retryableCodes = ['ECONNABORTED', 'ENOTFOUND', 'ECONNRESET', 'ETIMEDOUT'];
    const errorCode = typeof errorObj?.code === 'string' ? errorObj.code : undefined;
    return typeof errorCode === 'string' && retryableCodes.includes(errorCode);
  }

  private calculateDelay(attempt: number): number {
    // Exponential backoff: 1s, 2s, 4s
    return Math.min(1000 * Math.pow(2, attempt), 10000);
  }

  private async delay(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }

  private generateRequestId(): string {
    return `req_${Date.now()}_${Math.random().toString(36).substr(2, 6)}`;
  }

  private logRequest(requestId: string, serviceKey: string, config: AxiosRequestConfig): void {
    this.logger.log({
      message: 'External API request initiated',
      requestId,
      serviceKey,
      method: config.method?.toUpperCase(),
      url: config.url,
    });
  }

  private logSuccess(
    requestId: string,
    serviceKey: string,
    response: AxiosResponse,
    startTime: number
  ): void {
    this.logger.log({
      message: 'External API request completed',
      requestId,
      serviceKey,
      status: response.status,
      duration: Date.now() - startTime,
    });
  }

  private logError(requestId: string, serviceKey: string, error: any, startTime: number): void {
    const errorObj = error as Record<string, any>;
    let status: number | undefined;
    if (
      errorObj?.response &&
      typeof errorObj.response === 'object' &&
      'status' in errorObj.response
    ) {
      const responseObj = errorObj.response as Record<string, any>;
      status =
        typeof responseObj.status === 'number' ? responseObj.status : Number(responseObj.status);
    }

    this.logger.error({
      message: 'External API request failed',
      requestId,
      serviceKey,
      error: error instanceof Error ? error.message : 'Unknown error',
      status,
      duration: Date.now() - startTime,
    });
  }
}
