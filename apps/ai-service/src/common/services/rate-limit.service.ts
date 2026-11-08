import { Injectable, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';

/**
 * Rate limiting and quota management service for external APIs
 */
@Injectable()
export class RateLimitService {
  private readonly logger = new Logger(RateLimitService.name);
  private serviceLimits = new Map<string, ServiceLimitTracker>();

  constructor(private readonly configService: ConfigService) {}

  /**
   * Check if request is within rate limits for a service
   */
  checkRateLimit(serviceName: string, identifier?: string): RateLimitResult {
    const key = identifier ? `${serviceName}:${identifier}` : serviceName;
    const tracker = this.getOrCreateTracker(key, serviceName);

    const now = Date.now();
    const windowStart = now - tracker.windowMs;

    // Clean old requests
    tracker.requests = tracker.requests.filter((timestamp) => timestamp > windowStart);

    // Check if within limits
    if (tracker.requests.length >= tracker.limit) {
      const oldestRequest = Math.min(...tracker.requests);
      const resetTime = oldestRequest + tracker.windowMs;

      this.logger.warn(
        `Rate limit exceeded for ${key}. Limit: ${tracker.limit}/${tracker.windowMs}ms`
      );

      return {
        allowed: false,
        limit: tracker.limit,
        remaining: 0,
        resetTime: new Date(resetTime),
        retryAfter: Math.ceil((resetTime - now) / 1000),
      };
    }

    // Add current request
    tracker.requests.push(now);

    return {
      allowed: true,
      limit: tracker.limit,
      remaining: tracker.limit - tracker.requests.length,
      resetTime: new Date(now + tracker.windowMs),
      retryAfter: 0,
    };
  }

  /**
   * Get quota usage for a service
   */
  getQuotaUsage(serviceName: string, identifier?: string): QuotaUsage {
    const key = identifier ? `${serviceName}:${identifier}` : serviceName;
    const tracker = this.serviceLimits.get(key);

    if (!tracker) {
      return {
        serviceName,
        identifier,
        used: 0,
        limit: this.getDefaultLimit(serviceName),
        remaining: this.getDefaultLimit(serviceName),
        resetTime: new Date(Date.now() + this.getDefaultWindow()),
      };
    }

    const now = Date.now();
    const windowStart = now - tracker.windowMs;
    const currentRequests = tracker.requests.filter((timestamp) => timestamp > windowStart);

    return {
      serviceName,
      identifier,
      used: currentRequests.length,
      limit: tracker.limit,
      remaining: tracker.limit - currentRequests.length,
      resetTime: new Date(now + tracker.windowMs),
    };
  }

  /**
   * Set custom rate limit for a service
   */
  setRateLimit(serviceName: string, limit: number, windowMs: number): void {
    const config = this.getServiceConfig(serviceName);
    config.limit = limit;
    config.windowMs = windowMs;

    this.logger.log(`Updated rate limit for ${serviceName}: ${limit}/${windowMs}ms`);
  }

  /**
   * Reset rate limit tracking for a service
   */
  resetRateLimit(serviceName: string, identifier?: string): void {
    const key = identifier ? `${serviceName}:${identifier}` : serviceName;
    const tracker = this.serviceLimits.get(key);

    if (tracker) {
      tracker.requests = [];
      this.logger.log(`Reset rate limit tracking for ${key}`);
    }
  }

  /**
   * Get all service usage statistics
   */
  getAllUsageStats(): Record<string, QuotaUsage> {
    const stats: Record<string, QuotaUsage> = {};

    for (const [key, tracker] of this.serviceLimits) {
      const [serviceName, identifier] = key.includes(':') ? key.split(':', 2) : [key, undefined];
      const now = Date.now();
      const windowStart = now - tracker.windowMs;
      const currentRequests = tracker.requests.filter((timestamp) => timestamp > windowStart);

      stats[key] = {
        serviceName,
        identifier,
        used: currentRequests.length,
        limit: tracker.limit,
        remaining: tracker.limit - currentRequests.length,
        resetTime: new Date(now + tracker.windowMs),
      };
    }

    return stats;
  }

  /**
   * Clean up old tracking data
   */
  cleanup(): void {
    const now = Date.now();
    let cleanedCount = 0;

    for (const [key, tracker] of this.serviceLimits) {
      const windowStart = now - tracker.windowMs;
      const oldLength = tracker.requests.length;
      tracker.requests = tracker.requests.filter((timestamp) => timestamp > windowStart);

      if (tracker.requests.length === 0 && oldLength > 0) {
        this.serviceLimits.delete(key);
        cleanedCount++;
      }
    }

    if (cleanedCount > 0) {
      this.logger.debug(`Cleaned up ${cleanedCount} inactive rate limit trackers`);
    }
  }

  private getOrCreateTracker(key: string, serviceName: string): ServiceLimitTracker {
    let tracker = this.serviceLimits.get(key);

    if (!tracker) {
      const config = this.getServiceConfig(serviceName);
      tracker = {
        limit: config.limit,
        windowMs: config.windowMs,
        requests: [],
      };
      this.serviceLimits.set(key, tracker);
    }

    return tracker;
  }

  private getServiceConfig(serviceName: string): { limit: number; windowMs: number } {
    const configKey = `RATE_LIMIT_${serviceName.toUpperCase()}`;
    const limit =
      this.configService.get<number>(`${configKey}_LIMIT`) || this.getDefaultLimit(serviceName);
    const windowMs =
      this.configService.get<number>(`${configKey}_WINDOW`) || this.getDefaultWindow();

    return { limit, windowMs };
  }

  private getDefaultLimit(serviceName: string): number {
    const defaults: Record<string, number> = {
      openai: 60,
      azure_speech: 100,
      default: 100,
    };

    return defaults[serviceName.toLowerCase()] || defaults.default;
  }

  private getDefaultWindow(): number {
    // Default to 1 minute window
    return this.configService.get<number>('RATE_LIMIT_DEFAULT_WINDOW') || 60000;
  }
}

interface ServiceLimitTracker {
  limit: number;
  windowMs: number;
  requests: number[];
}

export interface RateLimitResult {
  allowed: boolean;
  limit: number;
  remaining: number;
  resetTime: Date;
  retryAfter: number;
}

export interface QuotaUsage {
  serviceName: string;
  identifier?: string;
  used: number;
  limit: number;
  remaining: number;
  resetTime: Date;
}
