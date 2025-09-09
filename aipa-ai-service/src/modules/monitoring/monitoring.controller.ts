import { Controller, Get } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse } from '@nestjs/swagger';
import { CircuitBreakerService } from '../../common/services/circuit-breaker.service';
import { RateLimitService } from '../../common/services/rate-limit.service';
import { OpenaiService } from '../openai/openai.service';
import { SpeechService } from '../speech/speech.service';

/**
 * Controller for monitoring external API integrations
 */
@ApiTags('monitoring')
@Controller('monitoring')
export class MonitoringController {
  constructor(
    private readonly circuitBreakerService: CircuitBreakerService,
    private readonly rateLimitService: RateLimitService,
    private readonly openaiService: OpenaiService,
    private readonly speechService: SpeechService
  ) {}

  /**
   * Get overall system health including external API status
   */
  @Get('health')
  @ApiOperation({ summary: 'Get system health status' })
  @ApiResponse({ status: 200, description: 'System health information' })
  getSystemHealth() {
    return {
      status: 'healthy',
      timestamp: new Date().toISOString(),
      services: {
        openai: {
          configured: this.openaiService.isConfigured(),
          stats: this.openaiService.getUsageStats(),
        },
        azureSpeech: {
          configured: this.speechService.isConfigured(),
          stats: this.speechService.getUsageStats(),
        },
      },
      circuitBreakers: this.circuitBreakerService.getAllStates(),
      rateLimits: this.rateLimitService.getAllUsageStats(),
    };
  }

  /**
   * Get circuit breaker states for all services
   */
  @Get('circuit-breakers')
  @ApiOperation({ summary: 'Get circuit breaker states' })
  @ApiResponse({ status: 200, description: 'Circuit breaker information' })
  getCircuitBreakerStates() {
    return {
      timestamp: new Date().toISOString(),
      circuitBreakers: this.circuitBreakerService.getAllStates(),
    };
  }

  /**
   * Get rate limiting information for all services
   */
  @Get('rate-limits')
  @ApiOperation({ summary: 'Get rate limiting status' })
  @ApiResponse({ status: 200, description: 'Rate limiting information' })
  getRateLimitStatus() {
    return {
      timestamp: new Date().toISOString(),
      rateLimits: this.rateLimitService.getAllUsageStats(),
    };
  }

  /**
   * Get OpenAI service statistics
   */
  @Get('openai-stats')
  @ApiOperation({ summary: 'Get OpenAI service statistics' })
  @ApiResponse({ status: 200, description: 'OpenAI service statistics' })
  getOpenAIStats() {
    return {
      timestamp: new Date().toISOString(),
      configured: this.openaiService.isConfigured(),
      usage: this.openaiService.getUsageStats(),
      circuitBreaker: {
        state: this.circuitBreakerService.getState('openai'),
        stats: this.circuitBreakerService.getStats('openai'),
      },
      rateLimit: this.rateLimitService.getQuotaUsage('openai'),
    };
  }

  /**
   * Get Azure Speech service statistics
   */
  @Get('speech-stats')
  @ApiOperation({ summary: 'Get Azure Speech service statistics' })
  @ApiResponse({ status: 200, description: 'Azure Speech service statistics' })
  getSpeechStats() {
    return {
      timestamp: new Date().toISOString(),
      configured: this.speechService.isConfigured(),
      usage: this.speechService.getUsageStats(),
      circuitBreaker: {
        transcription: {
          state: this.circuitBreakerService.getState('azure_speech'),
          stats: this.circuitBreakerService.getStats('azure_speech'),
        },
        textToSpeech: {
          state: this.circuitBreakerService.getState('azure_speech_tts'),
          stats: this.circuitBreakerService.getStats('azure_speech_tts'),
        },
      },
      rateLimit: this.rateLimitService.getQuotaUsage('azure_speech'),
    };
  }
}
