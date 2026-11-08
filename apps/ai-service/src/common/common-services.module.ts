import { Module, Global } from '@nestjs/common';
import { HttpModule } from '@nestjs/axios';
import { ConfigModule } from '@nestjs/config';
import { HttpClientService } from './services/http-client.service';
import { CircuitBreakerService } from './services/circuit-breaker.service';
import { RateLimitService } from './services/rate-limit.service';
import { PromptTemplateService } from './services/prompt-template.service';
import { FallbackQuestionService } from './services/fallback-question.service';
import { ResponseCacheService } from './services/response-cache.service';
import { ResponseValidationService } from './services/response-validation.service';

/**
 * Global module for common services used across the application
 */
@Global()
@Module({
  imports: [HttpModule, ConfigModule],
  providers: [
    HttpClientService,
    CircuitBreakerService,
    RateLimitService,
    PromptTemplateService,
    FallbackQuestionService,
    ResponseCacheService,
    ResponseValidationService,
  ],
  exports: [
    HttpClientService,
    CircuitBreakerService,
    RateLimitService,
    PromptTemplateService,
    FallbackQuestionService,
    ResponseCacheService,
    ResponseValidationService,
  ],
})
export class CommonServicesModule {}
