import { Injectable, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { ChatCompletionResponseDto } from '../../modules/openai/models/chat-completion-response.dto';

export interface CacheEntry<T> {
  data: T;
  timestamp: number;
  ttl: number; // Time to live in milliseconds
  accessCount: number;
  lastAccessed: number;
}

export interface CacheStats {
  totalEntries: number;
  hitCount: number;
  missCount: number;
  hitRate: number;
  memoryUsage: number; // Estimated memory usage in bytes
}

/**
 * Simple in-memory cache service for AI responses with TTL and LRU eviction
 */
@Injectable()
export class ResponseCacheService {
  private readonly logger = new Logger(ResponseCacheService.name);
  private readonly cache: Map<string, CacheEntry<any>> = new Map();
  private readonly maxSize: number;
  private readonly defaultTtl: number;
  private hitCount = 0;
  private missCount = 0;

  constructor(private readonly configService: ConfigService) {
    this.maxSize = this.configService.get<number>('CACHE_MAX_SIZE') || 1000;
    this.defaultTtl = this.configService.get<number>('CACHE_DEFAULT_TTL') || 300000; // 5 minutes

    // Start cleanup timer
    this.startCleanupTimer();

    this.logger.log(
      `Response cache initialized with max size: ${this.maxSize}, default TTL: ${this.defaultTtl}ms`
    );
  }

  /**
   * Generate cache key for AI responses
   */
  generateCacheKey(prompt: string, context?: Record<string, any>): string {
    const normalizedPrompt = prompt.toLowerCase().trim();
    const contextStr = context ? JSON.stringify(context) : '';

    // Simple hash function for cache key
    let hash = 0;
    const str = normalizedPrompt + contextStr;

    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = (hash << 5) - hash + char;
      hash = hash & hash; // Convert to 32-bit integer
    }

    return `ai_response_${Math.abs(hash)}`;
  }

  /**
   * Get cached response
   */
  get<T>(key: string): T | null {
    const entry = this.cache.get(key);

    if (!entry) {
      this.missCount++;
      return null;
    }

    // Check if entry has expired
    if (Date.now() - entry.timestamp > entry.ttl) {
      this.cache.delete(key);
      this.missCount++;
      return null;
    }

    // Update access statistics
    entry.accessCount++;
    entry.lastAccessed = Date.now();
    this.hitCount++;

    this.logger.debug(`Cache hit for key: ${key}`);
    return entry.data as T;
  }

  /**
   * Cache a response
   */
  set<T>(key: string, data: T, ttl?: number): void {
    // Check cache size and evict if necessary
    if (this.cache.size >= this.maxSize) {
      this.evictLeastRecentlyUsed();
    }

    const entry: CacheEntry<T> = {
      data,
      timestamp: Date.now(),
      ttl: ttl || this.defaultTtl,
      accessCount: 1,
      lastAccessed: Date.now(),
    };

    this.cache.set(key, entry);
    this.logger.debug(`Cached response for key: ${key}, TTL: ${entry.ttl}ms`);
  }

  /**
   * Get cached AI response with automatic key generation
   */
  getAIResponse(prompt: string, context?: Record<string, any>): ChatCompletionResponseDto | null {
    const key = this.generateCacheKey(prompt, context);
    return this.get<ChatCompletionResponseDto>(key);
  }

  /**
   * Cache AI response with automatic key generation
   */
  setAIResponse(
    prompt: string,
    response: ChatCompletionResponseDto,
    context?: Record<string, any>,
    ttl?: number
  ): void {
    const key = this.generateCacheKey(prompt, context);
    this.set(key, response, ttl);
  }

  /**
   * Check if a response exists in cache
   */
  has(key: string): boolean {
    const entry = this.cache.get(key);

    if (!entry) {
      return false;
    }

    // Check if entry has expired
    if (Date.now() - entry.timestamp > entry.ttl) {
      this.cache.delete(key);
      return false;
    }

    return true;
  }

  /**
   * Delete a cached entry
   */
  delete(key: string): boolean {
    return this.cache.delete(key);
  }

  /**
   * Clear all cached entries
   */
  clear(): void {
    this.cache.clear();
    this.hitCount = 0;
    this.missCount = 0;
    this.logger.log('Cache cleared');
  }

  /**
   * Get cache statistics
   */
  getStats(): CacheStats {
    const totalRequests = this.hitCount + this.missCount;
    const hitRate = totalRequests > 0 ? this.hitCount / totalRequests : 0;

    // Estimate memory usage (rough calculation)
    let memoryUsage = 0;
    for (const [key, entry] of this.cache.entries()) {
      memoryUsage += key.length * 2; // String character size
      memoryUsage += JSON.stringify(entry.data).length * 2; // Data size estimate
      memoryUsage += 64; // Overhead for entry object
    }

    return {
      totalEntries: this.cache.size,
      hitCount: this.hitCount,
      missCount: this.missCount,
      hitRate: Math.round(hitRate * 100) / 100,
      memoryUsage,
    };
  }

  /**
   * Get cache entries for debugging
   */
  getEntries(): Array<{ key: string; entry: CacheEntry<any> }> {
    return Array.from(this.cache.entries()).map(([key, entry]) => ({ key, entry }));
  }

  /**
   * Evict least recently used entries
   */
  private evictLeastRecentlyUsed(): void {
    if (this.cache.size === 0) {
      return;
    }

    let oldestKey = '';
    let oldestTime = Date.now();

    for (const [key, entry] of this.cache.entries()) {
      if (entry.lastAccessed < oldestTime) {
        oldestTime = entry.lastAccessed;
        oldestKey = key;
      }
    }

    if (oldestKey) {
      this.cache.delete(oldestKey);
      this.logger.debug(`Evicted least recently used cache entry: ${oldestKey}`);
    }
  }

  /**
   * Clean up expired entries
   */
  private cleanupExpiredEntries(): void {
    const now = Date.now();
    const expiredKeys: string[] = [];

    for (const [key, entry] of this.cache.entries()) {
      if (now - entry.timestamp > entry.ttl) {
        expiredKeys.push(key);
      }
    }

    for (const key of expiredKeys) {
      this.cache.delete(key);
    }

    if (expiredKeys.length > 0) {
      this.logger.debug(`Cleaned up ${expiredKeys.length} expired cache entries`);
    }
  }

  /**
   * Start automatic cleanup timer
   */
  private startCleanupTimer(): void {
    const cleanupInterval = this.configService.get<number>('CACHE_CLEANUP_INTERVAL') || 60000; // 1 minute

    setInterval(() => {
      this.cleanupExpiredEntries();
    }, cleanupInterval);

    this.logger.debug(`Cache cleanup timer started with interval: ${cleanupInterval}ms`);
  }
}
