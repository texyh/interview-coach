import { Injectable, Logger } from '@nestjs/common';
import { ChatCompletionResponseDto } from '../../modules/openai/models/chat-completion-response.dto';

export interface ValidationResult {
  isValid: boolean;
  issues: string[];
  severity: 'low' | 'medium' | 'high';
  shouldReject: boolean;
}

export interface ContentFilter {
  name: string;
  pattern: RegExp;
  severity: 'low' | 'medium' | 'high';
  description: string;
}

/**
 * Service for validating AI responses before sending to users
 */
@Injectable()
export class ResponseValidationService {
  private readonly logger = new Logger(ResponseValidationService.name);
  private readonly contentFilters: ContentFilter[] = [];
  private readonly profanityWords: string[] = [];
  private readonly minResponseLength: number = 10;
  private readonly maxResponseLength: number = 5000;

  constructor() {
    this.initializeContentFilters();
    this.initializeProfanityWords();
  }

  /**
   * Validate a chat completion response
   */
  validateResponse(response: ChatCompletionResponseDto): ValidationResult {
    const issues: string[] = [];
    let maxSeverity: 'low' | 'medium' | 'high' = 'low';

    // Basic validation
    const basicValidation = this.validateBasicStructure(response);
    if (!basicValidation.isValid) {
      issues.push(...basicValidation.issues);
      maxSeverity = this.combineSeverity(maxSeverity, basicValidation.severity);
    }

    // Content validation
    const contentValidation = this.validateContent(response.response);
    if (!contentValidation.isValid) {
      issues.push(...contentValidation.issues);
      maxSeverity = this.combineSeverity(maxSeverity, contentValidation.severity);
    }

    // Quality validation
    const qualityValidation = this.validateQuality(response);
    if (!qualityValidation.isValid) {
      issues.push(...qualityValidation.issues);
      maxSeverity = this.combineSeverity(maxSeverity, qualityValidation.severity);
    }

    const isValid = issues.length === 0;
    const shouldReject = maxSeverity === 'high' || issues.length > 5;

    if (!isValid) {
      this.logger.warn(`Response validation failed: ${issues.join(', ')}`);
    }

    return {
      isValid,
      issues,
      severity: maxSeverity,
      shouldReject,
    };
  }

  /**
   * Validate basic response structure
   */
  private validateBasicStructure(response: ChatCompletionResponseDto): ValidationResult {
    const issues: string[] = [];

    if (!response) {
      issues.push('Response object is null or undefined');
      return { isValid: false, issues, severity: 'high', shouldReject: true };
    }

    if (!response.response || typeof response.response !== 'string') {
      issues.push('Response text is missing or not a string');
      return { isValid: false, issues, severity: 'high', shouldReject: true };
    }

    if (response.response.trim().length === 0) {
      issues.push('Response text is empty');
      return { isValid: false, issues, severity: 'high', shouldReject: true };
    }

    if (response.response.length < this.minResponseLength) {
      issues.push(
        `Response too short (${response.response.length} chars, minimum ${this.minResponseLength})`
      );
    }

    if (response.response.length > this.maxResponseLength) {
      issues.push(
        `Response too long (${response.response.length} chars, maximum ${this.maxResponseLength})`
      );
    }

    return {
      isValid: issues.length === 0,
      issues,
      severity: issues.length > 0 ? 'medium' : 'low',
      shouldReject: false,
    };
  }

  /**
   * Validate response content for inappropriate material
   */
  private validateContent(responseText: string): ValidationResult {
    const issues: string[] = [];
    let maxSeverity: 'low' | 'medium' | 'high' = 'low';

    const normalizedText = responseText.toLowerCase();

    // Check against content filters
    for (const filter of this.contentFilters) {
      if (filter.pattern.test(normalizedText)) {
        issues.push(`Content filter triggered: ${filter.description}`);
        maxSeverity = this.combineSeverity(maxSeverity, filter.severity);
      }
    }

    // Check for profanity
    for (const word of this.profanityWords) {
      if (normalizedText.includes(word)) {
        issues.push(`Inappropriate language detected: ${word}`);
        maxSeverity = this.combineSeverity(maxSeverity, 'high');
      }
    }

    // Check for potential harmful content patterns
    const harmfulPatterns = [
      /\b(kill|murder|suicide|harm)\s+(yourself|others)\b/,
      /\b(illegal|unlawful)\s+(activities|actions)\b/,
      /\b(personal|private)\s+(information|data)\b/,
    ];

    for (const pattern of harmfulPatterns) {
      if (pattern.test(normalizedText)) {
        issues.push('Potentially harmful content detected');
        maxSeverity = this.combineSeverity(maxSeverity, 'high');
      }
    }

    return {
      isValid: issues.length === 0,
      issues,
      severity: maxSeverity,
      shouldReject: maxSeverity === 'high',
    };
  }

  /**
   * Validate response quality
   */
  private validateQuality(response: ChatCompletionResponseDto): ValidationResult {
    const issues: string[] = [];
    const responseText = response.response;

    // Check for coherence issues
    if (this.hasCoherenceIssues(responseText)) {
      issues.push('Response may lack coherence');
    }

    // Check for repetitive content
    if (this.hasRepetitiveContent(responseText)) {
      issues.push('Response contains repetitive content');
    }

    // Check for placeholder text
    if (this.hasPlaceholderText(responseText)) {
      issues.push('Response contains placeholder text');
    }

    // Check quality score if available
    if (response.qualityScore !== undefined && response.qualityScore < 0.3) {
      issues.push(`Low quality score: ${response.qualityScore}`);
    }

    return {
      isValid: issues.length === 0,
      issues,
      severity: issues.length > 2 ? 'medium' : 'low',
      shouldReject: false,
    };
  }

  /**
   * Check for coherence issues in text
   */
  private hasCoherenceIssues(text: string): boolean {
    // Simple heuristics for coherence
    const sentences = text.split(/[.!?]+/).filter((s) => s.trim().length > 0);

    if (sentences.length === 0) return true;

    // Check for very short sentences that might indicate incoherence
    const shortSentences = sentences.filter((s) => s.trim().split(/\s+/).length < 3);

    return shortSentences.length > sentences.length * 0.5;
  }

  /**
   * Check for repetitive content
   */
  private hasRepetitiveContent(text: string): boolean {
    const words = text.toLowerCase().split(/\s+/);
    const wordCount = new Map<string, number>();

    for (const word of words) {
      if (word.length > 3) {
        // Only count meaningful words
        wordCount.set(word, (wordCount.get(word) || 0) + 1);
      }
    }

    // Check if any word appears too frequently
    for (const [, count] of wordCount) {
      if (count > Math.max(5, words.length * 0.1)) {
        return true;
      }
    }

    return false;
  }

  /**
   * Check for placeholder text
   */
  private hasPlaceholderText(text: string): boolean {
    const placeholderPatterns = [
      /\[.*?\]/g,
      /\{.*?\}/g,
      /lorem ipsum/i,
      /placeholder/i,
      /\btbd\b/i,
      /\btodo\b/i,
      /xxx+/i,
    ];

    return placeholderPatterns.some((pattern) => pattern.test(text));
  }

  /**
   * Combine severity levels
   */
  private combineSeverity(
    current: 'low' | 'medium' | 'high',
    new_: 'low' | 'medium' | 'high'
  ): 'low' | 'medium' | 'high' {
    const severityOrder = { low: 1, medium: 2, high: 3 };
    return severityOrder[new_] > severityOrder[current] ? new_ : current;
  }

  /**
   * Initialize content filters
   */
  private initializeContentFilters(): void {
    this.contentFilters.push(
      {
        name: 'political_content',
        pattern: /\b(election|vote|democrat|republican|liberal|conservative)\b/i,
        severity: 'medium',
        description: 'Political content detected',
      },
      {
        name: 'religious_content',
        pattern: /\b(god|religion|bible|quran|church|mosque|temple)\b/i,
        severity: 'low',
        description: 'Religious content detected',
      },
      {
        name: 'financial_advice',
        pattern: /\b(invest|stock|crypto|financial advice|buy|sell)\b/i,
        severity: 'medium',
        description: 'Financial advice content detected',
      },
      {
        name: 'medical_advice',
        pattern: /\b(diagnose|treatment|medicine|drug|prescription)\b/i,
        severity: 'high',
        description: 'Medical advice content detected',
      },
      {
        name: 'personal_info',
        pattern: /\b(ssn|social security|credit card|password|phone number)\b/i,
        severity: 'high',
        description: 'Personal information reference detected',
      }
    );

    this.logger.log(`Initialized ${this.contentFilters.length} content filters`);
  }

  /**
   * Initialize profanity word list (minimal list for demo)
   */
  private initializeProfanityWords(): void {
    // This would typically be loaded from a more comprehensive external source
    this.profanityWords.push(
      'damn',
      'hell',
      'shit',
      'fuck',
      'ass',
      'bitch',
      'bastard'
      // Add more as needed, but keep it professional for work context
    );

    this.logger.log(`Initialized profanity filter with ${this.profanityWords.length} words`);
  }
}
