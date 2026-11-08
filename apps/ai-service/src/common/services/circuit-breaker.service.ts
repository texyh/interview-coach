import { Injectable, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import * as CircuitBreaker from 'opossum';

/**
 * Circuit breaker service for external API resilience
 */
@Injectable()
export class CircuitBreakerService {
  private readonly logger = new Logger(CircuitBreakerService.name);
  private circuitBreakers = new Map<string, CircuitBreaker<any[], any>>();

  constructor(private readonly configService: ConfigService) {}

  /**
   * Get or create a circuit breaker for a specific service
   */
  getCircuitBreaker<T extends any[], R>(
    serviceName: string,
    action: (...args: T) => Promise<R>,
    options?: Partial<CircuitBreaker.Options>
  ): CircuitBreaker<T, R> {
    if (this.circuitBreakers.has(serviceName)) {
      return this.circuitBreakers.get(serviceName) as CircuitBreaker<T, R>;
    }

    const defaultOptions: CircuitBreaker.Options = {
      timeout: this.configService.get<number>('CIRCUIT_BREAKER_TIMEOUT') || 30000,
      errorThresholdPercentage:
        this.configService.get<number>('CIRCUIT_BREAKER_ERROR_THRESHOLD') || 50,
      resetTimeout: this.configService.get<number>('CIRCUIT_BREAKER_RESET_TIMEOUT') || 30000,
      rollingCountTimeout:
        this.configService.get<number>('CIRCUIT_BREAKER_ROLLING_TIMEOUT') || 10000,
      rollingCountBuckets: this.configService.get<number>('CIRCUIT_BREAKER_ROLLING_BUCKETS') || 10,
      name: serviceName,
      group: 'external-apis',
    };

    const circuitBreakerOptions = { ...defaultOptions, ...options };

    const circuitBreaker = new CircuitBreaker(action, circuitBreakerOptions);

    // Set up event listeners
    this.setupCircuitBreakerEvents(circuitBreaker, serviceName);

    this.circuitBreakers.set(serviceName, circuitBreaker);
    this.logger.log(`Circuit breaker created for service: ${serviceName}`);

    return circuitBreaker as CircuitBreaker<T, R>;
  }

  /**
   * Execute a function with circuit breaker protection
   */
  async execute<T extends any[], R>(
    serviceName: string,
    action: (...args: T) => Promise<R>,
    args: T,
    options?: Partial<CircuitBreaker.Options>
  ): Promise<R> {
    const circuitBreaker = this.getCircuitBreaker(serviceName, action, options);
    return circuitBreaker.fire(...args);
  }

  /**
   * Get circuit breaker statistics
   */
  getStats(serviceName: string): CircuitBreaker.Stats | null {
    const circuitBreaker = this.circuitBreakers.get(serviceName);
    return circuitBreaker ? circuitBreaker.stats : null;
  }

  /**
   * Get circuit breaker state
   */
  getState(serviceName: string): string | null {
    const circuitBreaker = this.circuitBreakers.get(serviceName);
    if (!circuitBreaker) return null;

    if (circuitBreaker.opened) return 'OPEN';
    if (circuitBreaker.closed) return 'CLOSED';
    return 'HALF_OPEN';
  }

  /**
   * Check if circuit breaker is open
   */
  isOpen(serviceName: string): boolean {
    const circuitBreaker = this.circuitBreakers.get(serviceName);
    return circuitBreaker ? circuitBreaker.opened : false;
  }

  /**
   * Check if circuit breaker is closed
   */
  isClosed(serviceName: string): boolean {
    const circuitBreaker = this.circuitBreakers.get(serviceName);
    return circuitBreaker ? circuitBreaker.closed : true;
  }

  /**
   * Manually open a circuit breaker
   */
  open(serviceName: string): void {
    const circuitBreaker = this.circuitBreakers.get(serviceName);
    if (circuitBreaker) {
      circuitBreaker.open();
      this.logger.warn(`Circuit breaker manually opened for service: ${serviceName}`);
    }
  }

  /**
   * Manually close a circuit breaker
   */
  close(serviceName: string): void {
    const circuitBreaker = this.circuitBreakers.get(serviceName);
    if (circuitBreaker) {
      circuitBreaker.close();
      this.logger.log(`Circuit breaker manually closed for service: ${serviceName}`);
    }
  }

  /**
   * Get all circuit breaker states
   */
  getAllStates(): Record<string, { state: string; stats: CircuitBreaker.Stats }> {
    const states: Record<string, { state: string; stats: CircuitBreaker.Stats }> = {};

    for (const [serviceName, circuitBreaker] of this.circuitBreakers) {
      let state: string;
      if (circuitBreaker.opened) state = 'OPEN';
      else if (circuitBreaker.closed) state = 'CLOSED';
      else state = 'HALF_OPEN';

      states[serviceName] = {
        state,
        stats: circuitBreaker.stats,
      };
    }

    return states;
  }

  /**
   * Setup event listeners for circuit breaker
   */
  private setupCircuitBreakerEvents(
    circuitBreaker: CircuitBreaker<any[], any>,
    serviceName: string
  ): void {
    circuitBreaker.on('open', () => {
      this.logger.error(`Circuit breaker OPENED for service: ${serviceName}`);
    });

    circuitBreaker.on('close', () => {
      this.logger.log(`Circuit breaker CLOSED for service: ${serviceName}`);
    });

    circuitBreaker.on('halfOpen', () => {
      this.logger.warn(`Circuit breaker HALF-OPEN for service: ${serviceName}`);
    });

    circuitBreaker.on('failure', (error: Error) => {
      this.logger.error(`Circuit breaker failure for service ${serviceName}:`, error.message);
    });

    circuitBreaker.on('success', () => {
      this.logger.debug(`Circuit breaker success for service: ${serviceName}`);
    });

    circuitBreaker.on('timeout', () => {
      this.logger.error(`Circuit breaker timeout for service: ${serviceName}`);
    });

    circuitBreaker.on('reject', () => {
      this.logger.error(`Circuit breaker rejected request for service: ${serviceName}`);
    });

    circuitBreaker.on('fallback', (result: any) => {
      this.logger.warn(`Circuit breaker fallback triggered for service: ${serviceName}`, result);
    });
  }
}
