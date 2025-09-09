import { Injectable, BadRequestException, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import OpenAI from 'openai';
import { GenerateResponseDto } from './models/generate-response.dto';
import { ChatCompletionResponseDto } from './models/chat-completion-response.dto';
import { HttpClientService } from '../../common/services/http-client.service';
import { CircuitBreakerService } from '../../common/services/circuit-breaker.service';
import { RateLimitService } from '../../common/services/rate-limit.service';
import { PromptTemplateService } from '../../common/services/prompt-template.service';
import { FallbackQuestionService } from '../../common/services/fallback-question.service';
import { ResponseCacheService } from '../../common/services/response-cache.service';
import { ResponseValidationService } from '../../common/services/response-validation.service';

/**
 * Enhanced service for handling OpenAI API interactions with retry logic and monitoring
 */
@Injectable()
export class OpenaiService {
  private readonly logger = new Logger(OpenaiService.name);
  private readonly openai: OpenAI | null;
  private requestCount = 0;
  private errorCount = 0;

  constructor(
    private readonly configService: ConfigService,
    private readonly httpClientService: HttpClientService,
    private readonly circuitBreakerService: CircuitBreakerService,
    private readonly rateLimitService: RateLimitService,
    private readonly promptTemplateService: PromptTemplateService,
    private readonly fallbackQuestionService: FallbackQuestionService,
    private readonly responseCacheService: ResponseCacheService,
    private readonly responseValidationService: ResponseValidationService
  ) {
    const apiKey = this.configService.get<string>('OPENAI_API_KEY');

    if (!apiKey) {
      this.logger.warn('OpenAI API key not configured. Service will use mock responses.');
      this.openai = null;
    } else {
      this.openai = new OpenAI({
        apiKey,
        timeout: this.configService.get<number>('OPENAI_TIMEOUT') || 30000,
        maxRetries: this.configService.get<number>('OPENAI_MAX_RETRIES') || 3,
      });
    }
  }

  /**
   * Generates AI response for interview questions using OpenAI
   */
  async generateResponse(generateDto: GenerateResponseDto): Promise<ChatCompletionResponseDto> {
    if (!generateDto.prompt || generateDto.prompt.trim().length === 0) {
      throw new BadRequestException('Prompt is required');
    }

    // Check cache first
    const cacheContext = {
      temperature: generateDto.temperature,
      maxTokens: generateDto.maxTokens,
      context: generateDto.context,
    };

    const cachedResponse = this.responseCacheService.getAIResponse(
      generateDto.prompt,
      cacheContext
    );
    if (cachedResponse) {
      this.logger.debug('Returning cached response for prompt');
      return cachedResponse;
    }

    // Check rate limits
    const rateLimitResult = this.rateLimitService.checkRateLimit('openai');
    if (!rateLimitResult.allowed) {
      const error = new BadRequestException('Rate limit exceeded. Please try again later.');
      this.logger.warn(`OpenAI rate limit exceeded. Retry after: ${rateLimitResult.retryAfter}s`);
      throw error;
    }

    this.incrementRequestCount();

    // If no API key configured, return fallback response
    if (!this.openai) {
      this.logger.debug('Using fallback response - OpenAI API key not configured');
      return this.createFallbackResponse(generateDto);
    }

    // Use circuit breaker for OpenAI API calls
    try {
      const makeRequest = async (dto: GenerateResponseDto) => this.makeOpenAIRequest(dto);

      const result = await this.circuitBreakerService.execute(
        'openai',
        makeRequest,
        [generateDto],
        {
          timeout: 30000,
          errorThresholdPercentage: 50,
          resetTimeout: 30000,
        }
      );

      // Validate the response
      const validation = this.responseValidationService.validateResponse(result);
      if (validation.shouldReject) {
        this.logger.warn(`Response validation failed: ${validation.issues.join(', ')}`);
        return this.createFallbackResponse(generateDto);
      }

      // Cache the valid response
      this.responseCacheService.setAIResponse(generateDto.prompt, result, cacheContext);

      return result;
    } catch (error) {
      this.errorCount++;
      this.logger.error('Circuit breaker execution failed:', error);

      const errorObj = error as Record<string, any>;
      if (errorObj?.status === 401) {
        throw new BadRequestException('Invalid OpenAI API key');
      }

      if (errorObj?.status === 429) {
        throw new BadRequestException('OpenAI API rate limit exceeded');
      }

      // Fallback to mock response on API error
      this.logger.warn('Falling back to mock response due to API error');
      return this.createFallbackResponse(generateDto);
    }
  }

  /**
   * Make the actual OpenAI API request (used by circuit breaker)
   */
  private async makeOpenAIRequest(
    generateDto: GenerateResponseDto
  ): Promise<ChatCompletionResponseDto> {
    const completion = await this.openai!.chat.completions.create({
      model: this.configService.get<string>('OPENAI_MODEL') || 'gpt-3.5-turbo',
      messages: [
        {
          role: 'system',
          content: `You are an expert interview coach. Help the user improve their interview responses.
                   Be constructive, specific, and provide actionable feedback.`,
        },
        {
          role: 'user',
          content: generateDto.prompt,
        },
      ],
      max_tokens: generateDto.maxTokens || 500,
      temperature: generateDto.temperature || 0.7,
    });

    const response = completion.choices[0]?.message?.content || '';
    const tokensUsed = completion.usage?.total_tokens || 0;

    this.logApiUsage(tokensUsed);

    return {
      response,
      tokensUsed,
      qualityScore: this.calculateQualityScore(response),
      suggestions: this.extractSuggestions(response),
      timestamp: new Date(),
    };
  }

  /**
   * Analyzes interview response and provides feedback
   */
  async analyzeResponse(response: string, question: string): Promise<any> {
    if (!response || response.trim().length === 0) {
      throw new BadRequestException('Response is required for analysis');
    }

    if (!question || question.trim().length === 0) {
      throw new BadRequestException('Question is required for analysis');
    }

    if (!this.openai) {
      return this.createMockAnalysis();
    }

    try {
      const completion = await this.openai.chat.completions.create({
        model: this.configService.get<string>('OPENAI_MODEL') || 'gpt-3.5-turbo',
        messages: [
          {
            role: 'system',
            content: `You are an expert interview evaluator. Analyze the candidate's response to the interview question.
                     Provide a score (1-10), identify strengths, suggest improvements, and give overall feedback.
                     Respond in a structured JSON format.`,
          },
          {
            role: 'user',
            content: `Question: "${question}"\n\nCandidate Response: "${response}"\n\nPlease analyze this response.`,
          },
        ],
        max_tokens: 400,
        temperature: 0.3,
      });

      const analysis = completion.choices[0]?.message?.content || '';

      try {
        return JSON.parse(analysis);
      } catch {
        // If JSON parsing fails, return structured response
        return {
          score: 7.0,
          strengths: ['Response provided'],
          improvements: ['Could be more detailed'],
          overallFeedback: analysis,
          timestamp: new Date(),
        };
      }
    } catch (error) {
      this.logger.error('Error analyzing response:', error);
      return this.createMockAnalysis();
    }
  }

  /**
   * Check if OpenAI service is configured and available
   */
  isConfigured(): boolean {
    return !!this.openai;
  }

  /**
   * Get API usage statistics
   */
  getUsageStats(): { requestCount: number; errorCount: number; errorRate: number } {
    const errorRate = this.requestCount > 0 ? this.errorCount / this.requestCount : 0;
    return {
      requestCount: this.requestCount,
      errorCount: this.errorCount,
      errorRate: Math.round(errorRate * 100) / 100,
    };
  }

  private incrementRequestCount(): void {
    this.requestCount++;
  }

  private incrementErrorCount(): void {
    this.errorCount++;
  }

  private logApiUsage(tokensUsed: number): void {
    this.logger.log({
      message: 'OpenAI API request completed',
      tokensUsed,
      totalRequests: this.requestCount,
      errorRate: this.getUsageStats().errorRate,
    });
  }

  private createMockResponse(generateDto: GenerateResponseDto): ChatCompletionResponseDto {
    return {
      response: `This is a mock AI-generated response for: "${generateDto.prompt}"`,
      tokensUsed: 45,
      qualityScore: 0.85,
      suggestions: ['Add more specific examples', 'Include quantifiable achievements'],
      timestamp: new Date(),
    };
  }

  private createFallbackResponse(generateDto: GenerateResponseDto): ChatCompletionResponseDto {
    // Try to use a contextual fallback based on prompt content
    const prompt = generateDto.prompt.toLowerCase();

    let fallbackResponse = '';

    if (prompt.includes('tell me about yourself') || prompt.includes('introduce yourself')) {
      fallbackResponse =
        'I am a dedicated professional with experience in my field. I bring strong problem-solving skills, attention to detail, and a collaborative approach to team environments. I am passionate about continuous learning and contributing to meaningful projects.';
    } else if (prompt.includes('strength') || prompt.includes('what are you good at')) {
      fallbackResponse =
        'My key strengths include strong analytical thinking, effective communication, and the ability to work well under pressure. I am particularly good at breaking down complex problems into manageable components and finding practical solutions.';
    } else if (prompt.includes('weakness') || prompt.includes('area for improvement')) {
      fallbackResponse =
        'I am always working to improve my skills. One area I focus on is staying current with new technologies and methodologies in my field. I actively seek feedback and take steps to address any knowledge gaps through training and practice.';
    } else if (
      prompt.includes('why') &&
      (prompt.includes('company') || prompt.includes('position'))
    ) {
      fallbackResponse =
        "I am interested in this opportunity because it aligns with my career goals and values. The role offers the chance to apply my skills in a meaningful way while contributing to the company's mission and continued success.";
    } else {
      // Generic professional response
      fallbackResponse =
        'Thank you for this question. This is an important consideration in any professional environment. I would approach this situation by first gathering all relevant information, considering the various stakeholders involved, and then developing a thoughtful strategy that aligns with best practices and organizational goals.';
    }

    return {
      response: fallbackResponse,
      tokensUsed: Math.floor(fallbackResponse.length / 4), // Rough estimate
      qualityScore: 0.75, // Slightly lower than AI-generated
      suggestions: [
        'Consider adding specific examples from your experience',
        'Include quantifiable achievements where possible',
        'Tailor your response to the specific company and role',
      ],
      timestamp: new Date(),
    };
  }

  private createMockAnalysis(): any {
    return {
      score: 8.5,
      strengths: ['Clear communication', 'Relevant experience'],
      improvements: ['Add more specific examples', 'Include metrics'],
      overallFeedback: 'Good response with room for improvement',
      timestamp: new Date(),
    };
  }

  private calculateQualityScore(response: string): number {
    // Simple quality score calculation based on response length and structure
    if (!response) return 0;

    const wordCount = response.split(' ').length;
    const hasStructure = response.includes('.') || response.includes(',');

    let score = 0.5; // Base score

    if (wordCount > 20) score += 0.2;
    if (wordCount > 50) score += 0.2;
    if (hasStructure) score += 0.1;

    return Math.min(score, 1.0);
  }

  private extractSuggestions(response: string): string[] {
    // Extract actionable suggestions from the response
    const suggestions: string[] = [];

    if (response.includes('specific')) {
      suggestions.push('Be more specific in your examples');
    }

    if (response.includes('quantify') || response.includes('metrics')) {
      suggestions.push('Include quantifiable achievements');
    }

    if (response.includes('structure')) {
      suggestions.push('Improve response structure');
    }

    return suggestions.length > 0 ? suggestions : ['Continue practicing your responses'];
  }
}
